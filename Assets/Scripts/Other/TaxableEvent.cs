using System;

public sealed class TaxableEvent
{
    public const string CSV_DATA_ORDER = "Transaction ID,Date,Amount";

    private readonly string _id;
    private readonly DateTime _date;
    private readonly decimal _amount;

    public string Id => _id;
    public DateTime Date => _date;
    public decimal Amount => _amount;

    public TaxableEvent(string id, DateTime date, decimal amount)
    {
        _id = id;
        _date = date;
        _amount = amount;
    }

    public override string ToString()
    {
        return _id + "," + _date + "," + _amount;
    }
}
