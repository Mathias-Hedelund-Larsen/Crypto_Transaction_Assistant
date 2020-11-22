using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoingeckoPriceData
{
    [Serializable]
    public sealed class MarketData
    {
        [JsonProperty]
        private Dictionary<string, decimal> current_price;

        [JsonProperty]
        private Dictionary<string, decimal> market_cap;

        [JsonProperty]
        private Dictionary<string, decimal> total_volume;

        public Dictionary<string, decimal> Current_price { get => current_price; set => current_price = value; }
        public Dictionary<string, decimal> Market_cap { get => market_cap; set => market_cap = value; }
        public Dictionary<string, decimal> Total_volume { get => total_volume; set => total_volume = value; }
    }
}
