using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionFromHash
    {
        [SerializeField]
        private int code;

        [SerializeField]
        private string hash;

        [SerializeField]
        private string height;

        [SerializeField]
        private string log;

        [SerializeField]
        private bool ok;

        [SerializeField]
        private TransactionTx tx;

        public int Code { get => code; }
        public string Hash { get => hash; }
        public string Height { get => height; }
        public string Log { get => log; }
        public bool Ok { get => ok; }
        public TransactionTx Tx { get => tx; }
    }
}