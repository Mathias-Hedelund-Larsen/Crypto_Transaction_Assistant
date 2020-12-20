using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public sealed class TransactionTracking 
{
    [JsonProperty]
    private readonly Dictionary<AddressAndChain, List<TransactionIdAndType>> _transactionHistory;

    public Dictionary<AddressAndChain, List<TransactionIdAndType>> TransactionHistory { get => _transactionHistory; }

    public TransactionTracking(Dictionary<AddressAndChain, List<TransactionIdAndType>> transactionHistory)
    {
        _transactionHistory = transactionHistory;
    }
}
