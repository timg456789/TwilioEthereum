using Newtonsoft.Json;

namespace TwilioEthereum
{
    public class AwsButtonJson
    {
        public static readonly string CLICK_TYPE_LONG = "LONG";
        public static readonly string CLICK_TYPE_SINGLE = "SINGLE";
        public static readonly string CLICK_TYPE_DOUBLE = "DOUBLE";
        
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("batteryVoltage")]
        public string BatteryVoltage { get; set; }

        [JsonProperty("clickType")]
        public string ClickType { get; set; }
    }
}
