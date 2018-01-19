using Newtonsoft.Json;

namespace TwilioEthereum
{
    public class AwsButtonJson
    {
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("batteryVoltage")]
        public string BatteryVoltage { get; set; }

        [JsonProperty("clickType")]
        public string ClickType { get; set; }
    }
}
