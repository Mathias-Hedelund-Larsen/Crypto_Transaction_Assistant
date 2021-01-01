using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public sealed class AddressInfo
{
    [JsonProperty]
    private readonly string _address;

    [JsonProperty]
    private readonly BlockChain _chain;

    [JsonProperty]
    private readonly List<TransactionIdAndType> _markedTransactions;

    [JsonIgnore]
    public string Address => _address;

    [JsonIgnore]
    public BlockChain Chain => _chain;

    [JsonIgnore]
    public List<TransactionIdAndType> MarkedTransactions => _markedTransactions;

    public AddressInfo(string address, BlockChain chain, List<TransactionIdAndType> markedTransactions)
    {
        _address = address;
        _chain = chain;
        _markedTransactions = markedTransactions;
    }
}
