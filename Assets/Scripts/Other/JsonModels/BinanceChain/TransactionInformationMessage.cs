using Newtonsoft.Json;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformationMessage
    {
        [JsonProperty]
        private string type;

        [JsonProperty]
        private TransactionInformationMessageValue value;

        public string Type { get => type; }
        public TransactionInformationMessageValue Value { get => value; }
    }
}
