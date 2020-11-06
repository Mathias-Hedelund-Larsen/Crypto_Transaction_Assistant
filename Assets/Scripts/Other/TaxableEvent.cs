using System;
using System.Globalization;

public sealed class TaxableEvent
{
    public const string CSV_DATA_ORDER = "Purchase transaction ID,Sale transaction ID,Date,Amount,Currency,Total";

    private readonly DateTime _date;
    private readonly decimal _amount;
    private readonly string _currency;
    private readonly string _saleTransactionID;
    private readonly string _purchaseTransactionID;
    private readonly decimal _totalAmount;

    public DateTime Date => _date;
    public decimal Amount => _amount;
    public string Currency => _currency;
    public decimal TotalAmount => _totalAmount;
    public string SaleTransactionID => _saleTransactionID;
    public string PurchaseTransactionID => _purchaseTransactionID;

    public TaxableEvent(string purchaseTransactionID, string saleTransactionID,  DateTime date, decimal amount, string currency, decimal totalAmount)
    {
        _date = date;
        _amount = amount;
        _currency = currency;
        _totalAmount = totalAmount;
        _saleTransactionID = saleTransactionID;
        _purchaseTransactionID = purchaseTransactionID;
    }

    public override string ToString()
    {
        return _purchaseTransactionID + "," + _saleTransactionID + "," + _date + ",\"" + _amount.ToString() + "\"," + _currency + ",\"" + 
            _totalAmount.ToString() + "\"";
    }
}
