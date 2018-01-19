using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwilioEthereum
{
    class EthereumRpcContainerJson
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public IEnumerable<string> Params { get; set; }
    }
}
