using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace TwilioEthereum
{
    public class Responses
    {
        public static APIGatewayProxyResponse EmptyResponse => new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = $@"<?xml version=""1.0"" encoding=""UTF-8""?><Response></Response>",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/xml; charset=utf-8" } }
        };

        /// <remarks>
        /// I figure it's best to return an API Gateway response and hide twilio.
        /// </remarks>
        public static APIGatewayProxyResponse ForbiddenResponse => new APIGatewayProxyResponse
        {
            StatusCode = (int) HttpStatusCode.Forbidden,
            Body = new JObject { { "message", "Missing Authentication Token" } }.ToString()
        };
    }
}
