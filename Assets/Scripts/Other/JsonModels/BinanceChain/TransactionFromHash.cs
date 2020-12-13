using System;
using Newtonsoft.Json;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionFromHash
    {
        [JsonProperty]
        private int code;

        [JsonProperty]
        private string hash;

        [JsonProperty]
        private string height;

        [JsonProperty]
        private string log;

        [JsonProperty]
        private bool ok;

        [JsonProperty]
        private TransactionTx tx;

        public int Code { get => code; }
        public string Hash { get => hash; }
        public string Height { get => height; }
        public string Log { get => log; }
        public bool Ok { get => ok; }
        public TransactionTx Tx { get => tx; }
    }
}