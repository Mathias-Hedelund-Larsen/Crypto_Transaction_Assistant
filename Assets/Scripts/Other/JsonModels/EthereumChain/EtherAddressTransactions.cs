using Newtonsoft.Json;
using System;

namespace EthereumChain
{
    [Serializable]
    public sealed class EtherAddressTransactions
    {
        [JsonProperty]
        private readonly string status;

        [JsonProperty]
        private readonly string message;

        [JsonProperty]
        private readonly EtherTransaction[] result;

        public string Status => status;

        public string Message => message;

        public EtherTransaction[] Result => result;
    }
}