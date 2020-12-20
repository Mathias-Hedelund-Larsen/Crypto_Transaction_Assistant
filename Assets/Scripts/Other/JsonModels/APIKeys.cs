using Newtonsoft.Json;
using System;

[Serializable]
public sealed class APIKeys 
{
    [JsonProperty]
    private readonly string _etherScanKey;

    [JsonIgnore]
    public string EtherScanKey => _etherScanKey;
}
