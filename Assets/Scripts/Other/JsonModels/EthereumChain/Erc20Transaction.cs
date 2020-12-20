using Newtonsoft.Json;
using System;

namespace EthereumChain
{
    [Serializable]
    public sealed class Erc20Transaction : IEtherScanTransaction
    {
        [JsonProperty]
        private readonly string blockNumber;

        [JsonProperty]
        private readonly string timeStamp;

        [JsonProperty]
        private readonly string hash;

        [JsonProperty]
        private readonly string nonce;

        [JsonProperty]
        private readonly string blockHash;

        [JsonProperty]
        private readonly string from;

        [JsonProperty]
        private readonly string contractAddress;

        [JsonProperty]
        private readonly string to;

        [JsonProperty]
        private readonly string value;

        [JsonProperty]
        private readonly string tokenName;

        [JsonProperty]
        private readonly string tokenSymbol;

        [JsonProperty]
        private readonly string tokenDecimal;

        [JsonProperty]
        private readonly string transactionIndex;

        [JsonProperty]
        private readonly string gas;

        [JsonProperty]
        private readonly string gasPrice;

        [JsonProperty]
        private readonly string gasUsed;

        [JsonProperty]
        private readonly string cumulativeGasUsed;

        [JsonProperty]
        private readonly string input;

        [JsonProperty]
        private readonly string confirmations;

        public string BlockNumber => blockNumber;

        public DateTime TimeStamp
        {
            get
            {
                DateTime epoch = new DateTime(1970, 1, 1);

                if (long.TryParse(timeStamp, out long secondsFromEpoch))
                {
                    return epoch.AddSeconds(secondsFromEpoch);
                }

                return epoch;
            }
        }

        public string Hash => hash;

        public string Nonce => nonce;

        public string BlockHash => blockHash;

        public string TransactionIndex => transactionIndex;

        public string From => from;

        public string To => to;

        public decimal Value
        {
            get
            {
                if (int.TryParse(tokenDecimal, out int decimals))
                {
                    int dividedBy = 10.Pow(decimals);

                    if (decimal.TryParse(value, out decimal result))
                    {
                        return result / dividedBy;
                    }
                }
                return decimal.Zero;
            }
        }

        public string Gas => gas;

        public string GasPrice => gasPrice;

        public string Input => input;

        public string ContractAddress => contractAddress;

        public string CumulativeGasUsed => cumulativeGasUsed;

        public string GasUsed => gasUsed;

        public string Confirmations => confirmations;

        public string TokenName => tokenName;

        public string TokenSymbol => tokenSymbol;
    }
}