using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace TwilioEthereum
{
    public class Relay
    {
        private readonly ILogging logging;
        private readonly string etherscanApiKey;
        private readonly IAmazonSQS queueClient;
        private readonly string queueUrl;
        private readonly HttpClient client;
        
        public Relay(
            ILogging logging,
            string etherscanApiKey,
            IAmazonSQS queueClient,
            string queuUrl)
        {
            this.logging = logging;
            this.etherscanApiKey = etherscanApiKey;
            this.queueClient = queueClient;
            this.queueUrl = queuUrl;
            this.client = new HttpClient();
        }

        public string ConfirmMessages()
        {
            var msgRequest = new ReceiveMessageRequest(queueUrl);
            msgRequest.MaxNumberOfMessages = 10;
            ReceiveMessageResponse messageBatch = queueClient.ReceiveMessageAsync(queueUrl).Result;
            if (!messageBatch.Messages.Any())
            {
                return "No pending transactions.";
            }

            var responses = (from m in messageBatch.Messages
                select new
                {
                    Message = m,
                    Response = RelayMessage(m.Body)
                }).ToList();
            Task.WaitAll(responses.Select(x => x.Response).ToArray());
            
            var failed = responses
                .Where(x => !x.Response.Result.IsSuccessStatusCode)
                .ToList();
            string response = "";
            if (failed.Any())
            {
                response += $"Failed to send {failed.Count} transactions.";
                logging.Log($"Failed to send {failed.Count} transactions.");
                logging.Log($"Responses: {failed.Select(x => x.Response.Result.StatusCode + " - " + x.Response.Result.Content.ReadAsStringAsync())}");
            }

            var deleteItems = responses
                .Where(x => x.Response.Result.IsSuccessStatusCode)
                .Select(x => new DeleteMessageBatchRequestEntry(x.Message.MessageId, x.Message.ReceiptHandle))
                .ToList();
            var deleteRequest = new DeleteMessageBatchRequest(queueUrl, deleteItems);
            if (deleteItems.Any())
            {
                response += $"Sent {deleteItems.Count} transactions.";
                var deleteResult = queueClient.DeleteMessageBatchAsync(deleteRequest).Result;
                if (deleteResult.Failed.Any())
                {
                    var deleteFailureReasons = deleteResult.Failed.Select(x => x.Message);
                    response += $"Failed to delete {deleteResult.Failed.Count} transactions from queue.";
                    logging.Log($"Failed to delete {deleteResult.Failed.Count} transactions from queue: " + string.Join(", ", deleteFailureReasons));
                }
            }

            return response;
        }
        
        protected virtual Task<HttpResponseMessage> RelayMessage(string transactionHex)
        {
            var url = $"https://api.etherscan.io/api?module=proxy&action=eth_sendRawTransaction&apikey={etherscanApiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var requestParams = new List<KeyValuePair<string, string>>();
            requestParams.Add(new KeyValuePair<string, string>("hex", transactionHex));
            request.Content = new FormUrlEncodedContent(requestParams);
            return client.SendAsync(request);
        }

    }
}
