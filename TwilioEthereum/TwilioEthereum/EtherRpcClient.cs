using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwilioEthereum
{
    public class EtherRpcClient
    {
        private static int requestId;
        private static readonly object requestIdLock = new object();

        private readonly string provider;
        private readonly HttpClient client = new HttpClient();

        public EtherRpcClient(string provider)
        {
            this.provider = provider;
        }

        public virtual Task<HttpResponseMessage> Broadcast(string signedTransactionHex)
        {
            var json = new EthereumRpcContainerJson();
            lock(requestIdLock)
            {
                json.Id = requestId;
                requestId += 1;
            }
            json.JsonRpc = "2.0";
            json.Method = "eth_sendRawTransaction";
            json.Params = new List<string> {signedTransactionHex};
            var request = new HttpRequestMessage(HttpMethod.Post, provider);
            request.Content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8);
            return client.SendAsync(request);
        }
    }
}
