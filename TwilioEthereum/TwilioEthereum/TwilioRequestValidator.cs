using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Twilio.Security;

namespace TwilioEthereum
{
    class TwilioRequestValidator
    {
        private readonly string twilioToken;

        public TwilioRequestValidator(string twilioToken)
        {
            this.twilioToken = twilioToken;
        }

        public virtual bool IsFromTwilio(JObject request, Dictionary<string, string> parameters)
        {
            try
            {
                var url = request["headers"]["X-Forwarded-Proto"].Value<string>() +
                          "://" + request["headers"]["Host"].Value<string>() + "/" +
                          request["requestContext"]["stage"].Value<string>() +
                          request["path"];
                var signature = request["headers"]["X-Twilio-Signature"].Value<string>();
                
                var requestValidator = new RequestValidator(twilioToken);
                return requestValidator.Validate(url, parameters, signature);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
