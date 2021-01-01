using Newtonsoft.Json;
using System;

[Serializable]
public sealed class TransactionIdAndType 
{
    [JsonProperty]
    private readonly string _transactionId;

    [JsonProperty]
    private readonly TransactionType _transactionType;

    [JsonProperty]
    private readonly decimal _manuelPrice;

    [JsonProperty]
    private readonly bool _useManuelPrice;

    public string TransactionId => _transactionId;

    public TransactionType TransactionType => _transactionType;

    public decimal Price => _manuelPrice;

    public bool UseManuelPrice => _useManuelPrice;

    public TransactionIdAndType(string transactionId, TransactionType transactionType, bool useManuelPrice = false, decimal price = 0)
    {
        _transactionId = transactionId;
        _transactionType = transactionType;
        _manuelPrice = price;
        _useManuelPrice = useManuelPrice;
    }
}
