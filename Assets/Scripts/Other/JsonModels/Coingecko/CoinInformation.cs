using Newtonsoft.Json;
using System;

namespace CoingeckoPriceData
{
    [Serializable]
    public class CoinInformation
    {
        [JsonProperty]
        private string id;

        [JsonProperty]
        private string symbol;

        [JsonProperty]
        private CoinImage image;

        [JsonProperty]
        private MarketData market_data;

        public string Id { get => id; set => id = value; }
        public string Symbol { get => symbol; set => symbol = value; }
        public CoinImage Image { get => image; set => image = value; }
        public MarketData Market_data { get => market_data; set => market_data = value; }
    }
}
