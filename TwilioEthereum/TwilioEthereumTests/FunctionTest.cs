using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json.Linq;
using TwilioEthereum;
using Xunit;

namespace TwilioEthereumTests
{
    public class FunctionTest
    {
        [Fact]
        public void TestSmsIn()
        {
            var config = PrivateConfig.CreateFromPersonalJson();
            Environment.SetEnvironmentVariable("twilioProductionSid", config.TwilioProductionSid);
            Environment.SetEnvironmentVariable("twilioProductionToken", config.TwilioProductionToken);



            var context = new TestLambdaContext();
            var function = new Function();
            function.FunctionHandler(new JObject(), context);
        }
    }
}
