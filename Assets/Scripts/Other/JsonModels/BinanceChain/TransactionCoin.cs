using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionCoin 
    {
        [SerializeField]
        private string amount;

        [SerializeField]
        private string denom;

        public string Amount { get => amount; }
        public string Denom { get => denom; }
    }
}
