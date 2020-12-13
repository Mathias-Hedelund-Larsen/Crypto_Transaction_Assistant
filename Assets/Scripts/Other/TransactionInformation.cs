using System;

public sealed class TransactionInformation 
{
    public string FileName { get; }
    public string EntryData { get; }
    public Type TransactionType { get; }

    public TransactionInformation(string fileName, string entryData, Type transactionType)
    {
        FileName = fileName;
        EntryData = entryData;
        TransactionType = transactionType;
    }
}
