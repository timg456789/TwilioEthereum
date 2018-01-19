using System.IO;
using Newtonsoft.Json;

namespace TwilioEthereumTests
{
    public class PrivateConfig
    {
        [JsonProperty("etherscanApiKey")]
        public string EtherscanApiKey { get; set; }

        [JsonProperty("infuraRinkeby")]
        public string InfuraRinkeby { get; set; }
        
        [JsonProperty("twilioSmsPotArn")]
        public string TwilioSmsPotArn { get; set; }
        
        [JsonProperty("twilioEthereumArn")]
        public string TwilioEthereumArn { get; set; }

        [JsonProperty("twilioProductionSid")]
        public string TwilioProductionSid { get; set; }

        [JsonProperty("twilioProductionToken")]
        public string TwilioProductionToken { get; set; }

        [JsonProperty("phoneNumberCellPhone")]
        public string PhoneNumberCellPhone { get; set; }

        [JsonProperty("phoneNumberTwilioPurchased")]
        public string PhoneNumberTwilioPurchased { get; set; }

        [JsonProperty("phoneNumberTwilioPurchasedNorthSanDiego")]
        public string PhoneNumberTwilioPurchasedNorthSanDiego { get; set; }

        [JsonProperty("phoneNumberTwilioPurchasedSouthSanDiego")]
        public string PhoneNumberTwilioPurchasedSouthSanDiego { get; set; }
        
        public static string PersonalJson => "C:\\Users\\peon\\Desktop\\projects\\Memex\\personal.json";

        public static PrivateConfig CreateFromPersonalJson()
        {
            return Create(PersonalJson);
        }

        private static PrivateConfig Create(string fullPath)
        {
            var json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<PrivateConfig>(json);
        }
    }
}
