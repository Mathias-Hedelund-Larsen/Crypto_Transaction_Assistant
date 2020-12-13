using Newtonsoft.Json;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformation
    {
        [JsonProperty]
        private string data;

        [JsonProperty]
        private string memo;

        [JsonProperty]
        private TransactionInformationMessage[] msg; 

        public string Data { get => data; }
        public string Memo { get => memo; }
        public TransactionInformationMessage[] Msg { get => msg; }
    }
}
