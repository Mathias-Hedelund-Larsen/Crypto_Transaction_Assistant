using System;
using UnityEngine;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionInformation
    {
        [SerializeField]
        private string data;

        [SerializeField]
        private string memo;

        [SerializeField]
        private TransactionInformationMessage[] msg; 

        public string Data { get => data; }
        public string Memo { get => memo; }
        public TransactionInformationMessage[] Msg { get => msg; }
    }
}
