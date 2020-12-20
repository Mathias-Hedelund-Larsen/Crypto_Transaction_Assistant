using Newtonsoft.Json;
using System;

[Serializable]
public sealed class AddressAndChain 
{
    [JsonProperty]
    private readonly string _address;

    [JsonProperty]
    private readonly BlockChain _chain;

    public string Address => _address;

    public BlockChain Chain => _chain;

    public AddressAndChain(string address, BlockChain chain)
    {
        _address = address;
        _chain = chain;
    }
}
