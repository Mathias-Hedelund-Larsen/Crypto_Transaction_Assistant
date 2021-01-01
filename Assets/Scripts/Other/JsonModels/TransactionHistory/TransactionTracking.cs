using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public sealed class TransactionTracking 
{
    [JsonProperty]
    private readonly List<AddressInfo> _transactionHistory;

    [JsonIgnore]
    public List<AddressInfo> TransactionHistory { get => _transactionHistory; }

    public TransactionTracking(List<AddressInfo> transactionHistory)
    {
        _transactionHistory = transactionHistory;
    }
}
