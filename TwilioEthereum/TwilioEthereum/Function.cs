using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TwilioEthereum
{
    public class Function
    {
        public static readonly string TWILIO_ETHEREUM_QUEUE = "TwilioEthereumQueue";
        public static readonly string TWILIO_ETHEREUM_QUEUE_URL = "https://sqs.us-east-1.amazonaws.com/363557355695/TwilioEthereumQueue";

        protected List<string> knownNumbers = new List<string>
        {
            Environment.GetEnvironmentVariable("phoneNumberTwilioPurchased"),
            Environment.GetEnvironmentVariable("phoneNumberTwilioPurchasedNorthSanDiego"),
            Environment.GetEnvironmentVariable("phoneNumberTwilioPurchasedSouthSanDiego"),
            Environment.GetEnvironmentVariable("phoneNumberCellPhone")
        };

        private readonly TwilioRequestValidator requestValidator =
            new TwilioRequestValidator(Environment.GetEnvironmentVariable("twilioProductionToken"));

        private readonly IAmazonSQS queueClient;
        private readonly string queueUrl;
        private readonly ILogging logging;

        public Function()
            : this(
                new AmazonSQSClient(new AmazonSQSConfig{ RegionEndpoint = RegionEndpoint.USEast1 }),
                TWILIO_ETHEREUM_QUEUE_URL,
                new ConsoleLogging()
            )
        {
        }

        public Function(IAmazonSQS queueClient, string queueUrl, ILogging logging)
        {
            this.queueClient = queueClient;
            this.queueUrl = queueUrl;
            this.logging = logging;
            TwilioClient.Init(Environment.GetEnvironmentVariable("twilioProductionSid"),
                Environment.GetEnvironmentVariable("twilioProductionToken"));
        }

        /// <remarks>
        /// Authenticating the button id isn't very important, the button has been granted explicit permission already.
        /// The endpoint however should be considered public.
        /// </remarks>
        public APIGatewayProxyResponse FunctionHandler(
            JObject json,
            ILambdaContext context)
        {
            Console.WriteLine("Incoming request: " + json);
            string from;
            string body;

            var buttonJson = JsonConvert.DeserializeObject<AwsButtonJson>(json.ToString());
            if (Environment.GetEnvironmentVariable("awsButtonId") == buttonJson.SerialNumber)
            {
                from = Environment.GetEnvironmentVariable("phoneNumberCellPhone");
                body = "confirm";
            }
            else
            {
                var parameters = json["body"].Value<string>().Split('&').ToDictionary(
                    x => WebUtility.UrlDecode(x.Split('=')[0]),
                    x => WebUtility.UrlDecode(x.Split('=')[1])
                );

                if (!requestValidator.IsFromTwilio(json, parameters))
                {
                    SendAdminTextMessage("unknown", json["body"].Value<string>());
                    return Responses.ForbiddenResponse;
                }
                from = parameters["From"];
                body = parameters["Body"];
                if (!knownNumbers.Any(from.Equals))
                {
                    SendAdminTextMessage(from, body);
                    return Responses.EmptyResponse;
                }
            }
            
            var relay = new Relay(
                new ConsoleLogging(), Environment.GetEnvironmentVariable("ethereumProvider"),
                queueClient, queueUrl);

            string response;
            if (body.StartsWith("0x"))
            {
                var result = queueClient.SendMessageAsync(queueUrl, body.Trim()).Result;
                response = $"Inserted transaction signed transaction hex into queue. Queue message md5 hash {result.MD5OfMessageBody}.";
            }
            else if (body.Equals("confirm", StringComparison.OrdinalIgnoreCase))
            {
                response = relay.ConfirmMessages();
            }
            else
            {
                response = "unknown command " + body;
            }

            MessageResource.Create(
                to: from,
                from: Environment.GetEnvironmentVariable("phoneNumberTwilioPurchasedSouthSanDiego"),
                body: response);
            
            return Responses.EmptyResponse;
        }

        public void SendAdminTextMessage(string from, string body)
        {
            logging.Log("Not from twilio purchased. Forwarding to default SMS number.");
            var knownMessage = $"DEFAULTED FROM {from}: {body}";
            logging.Log(knownMessage);
            MessageResource.Create(
                to: new PhoneNumber(Environment.GetEnvironmentVariable("phoneNumberCellPhone")),
                from: Environment.GetEnvironmentVariable("phoneNumberTwilioPurchasedSouthSanDiego"),
                body: knownMessage);
        }

    }
}
