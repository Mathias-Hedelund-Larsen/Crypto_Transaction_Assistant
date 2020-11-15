using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionAddressAndCoins 
    {
        [SerializeField]
        private string address;

        [SerializeField]
        private TransactionCoin[] coins;

        public string Address { get => address; }
        public TransactionCoin[] Coins { get => coins; }
    }
}
