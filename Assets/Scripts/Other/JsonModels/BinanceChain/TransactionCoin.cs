using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionCoin 
    {
        [SerializeField]
        private string denom;

        [SerializeField]
        private string amount;

        public string Denom { get => denom; }
        public decimal Amount { get => decimal.Parse(amount); }
    }
}
