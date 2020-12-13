using Newtonsoft.Json;
using System;

namespace BinanceChain 
{
    [Serializable]
    public sealed class TransactionTx
    {
        [JsonProperty]
        private string type;

        [JsonProperty]
        private TransactionInformation value;

        public string Type { get => type; }
        public TransactionInformation Value { get => value; }
    }
}
