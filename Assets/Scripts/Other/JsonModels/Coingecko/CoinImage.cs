using Newtonsoft.Json;
using System;

namespace CoingeckoPriceData
{
    [Serializable]
    public sealed class CoinImage
    {
        [JsonProperty]
        private string thumb;

        [JsonProperty]
        private string small;

        public string Thumb { get => thumb; set => thumb = value; }
        public string Small { get => small; set => small = value; }
    }
}
