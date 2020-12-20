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
    private readonly decimal _price;

    public string TransactionId => _transactionId;

    public TransactionType TransactionType => _transactionType;

    public decimal Price => _price;

    public TransactionIdAndType(string transactionId, TransactionType transactionType, decimal price)
    {
        _transactionId = transactionId;
        _transactionType = transactionType;
        _price = price;
    }
}
