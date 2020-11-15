using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformationMessage
    {
        [SerializeField]
        private string type;

        [SerializeField]
        private TransactionInformationMessageValue value;

        public string Type { get => type; }
        public TransactionInformationMessageValue Value { get => value; }
    }
}
