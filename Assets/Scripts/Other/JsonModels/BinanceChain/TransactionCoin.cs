using Newtonsoft.Json;
using System;

namespace BinanceChain
{
    [Serializable]
    public sealed class TransactionCoin 
    {
        [JsonProperty]
        private string denom;

        [JsonProperty]
        private string amount;

        [JsonIgnore]
        private decimal? _amount;

        public string Denom { get => denom; }
        public decimal Amount 
        {
            get 
            {
                if (_amount == null)
                {
                    _amount = decimal.Parse(amount);

                    switch (denom)
                    {
                        case "AWC-986":
                            _amount /= 100000000;
                            break;                      
                    }
                }

                return _amount.Value;
            }
        }
    }
}
