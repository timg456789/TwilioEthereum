using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace TwilioEthereum
{
    public class Relay
    {
        private readonly ILogging logging;
        private readonly string provider;
        private readonly IAmazonSQS queueClient;
        private readonly string queueUrl;
        
        public Relay(
            ILogging logging,
            string provider,
            IAmazonSQS queueClient,
            string queuUrl)
        {
            this.logging = logging;
            this.provider = provider;
            this.queueClient = queueClient;
            this.queueUrl = queuUrl;
        }

        public string ConfirmMessages()
        {
            var msgRequest = new ReceiveMessageRequest(queueUrl);
            msgRequest.MaxNumberOfMessages = 10;
            ReceiveMessageResponse messageBatch = queueClient.ReceiveMessageAsync(queueUrl).Result;
            if (!messageBatch.Messages.Any())
            {
                return "No unsent transactions.";
            }
            
            var client = new EtherRpcClient(provider);
            var responses = (from m in messageBatch.Messages
                select new
                {
                    Message = m,
                    Response = client.Broadcast(m.Body)
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

            var success = responses.Where(x => x.Response.Result.IsSuccessStatusCode).ToList();
            if (success.Any())
            {
                var deleteItems = success
                    .Select(x => new DeleteMessageBatchRequestEntry(x.Message.MessageId, x.Message.ReceiptHandle))
                    .ToList();
                var deleteRequest = new DeleteMessageBatchRequest(queueUrl, deleteItems);
                var successResponses = success.Select(x => x.Response.Result.Content.ReadAsStringAsync().Result);
                var successResponseText = string.Join(", ", successResponses);
                response += $"Broadcast {deleteItems.Count} transactions to {provider} {successResponseText}";
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

    }
}
