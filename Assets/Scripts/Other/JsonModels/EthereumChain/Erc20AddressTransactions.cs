using Newtonsoft.Json;
using System;

namespace EthereumChain
{
    [Serializable]
    public sealed class Erc20AddressTransactions
    {
        [JsonProperty]
        private readonly string status;

        [JsonProperty]
        private readonly string message;

        [JsonProperty]
        private readonly Erc20Transaction[] result;

        public string Status => status;

        public string Message => message;

        public Erc20Transaction[] Result => result;
    }
}