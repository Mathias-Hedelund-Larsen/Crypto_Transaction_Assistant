using System;
using UnityEngine;

namespace BinanceChain 
{
    [Serializable]
    public sealed class TransactionTx
    {
        [SerializeField]
        private string type;

        [SerializeField]
        private TransactionInformation value;

        public string Type { get => type; }
        public TransactionInformation Value { get => value; }
    }
}
