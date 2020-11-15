using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformationMessageValue
    {
        [SerializeField]
        private TransactionAddressAndCoins[] inputs;

        [SerializeField]
        private TransactionAddressAndCoins[] outputs;

        public TransactionAddressAndCoins[] Inputs { get => inputs; }
        public TransactionAddressAndCoins[] Outputs { get => outputs; }
    }
}
