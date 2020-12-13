using Newtonsoft.Json;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionAddressAndCoins 
    {
        [JsonProperty]
        private string address;

        [JsonProperty]
        private TransactionCoin[] coins;

        public string Address { get => address; }
        public TransactionCoin[] Coins { get => coins; }
    }
}
