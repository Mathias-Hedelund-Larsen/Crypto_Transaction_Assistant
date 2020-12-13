using Newtonsoft.Json;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformationMessageValue
    {
        [JsonProperty]
        private TransactionAddressAndCoins[] inputs;

        [JsonProperty]
        private TransactionAddressAndCoins[] outputs;

        public TransactionAddressAndCoins[] Inputs { get => inputs; }
        public TransactionAddressAndCoins[] Outputs { get => outputs; }
    }
}
