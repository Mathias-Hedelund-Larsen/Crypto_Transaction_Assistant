using System;
using System.Globalization;

public sealed class TaxableEvent
{
    public const string CSV_DATA_ORDER = "Purchase transaction ID,Sale transaction ID,Purchase date,Sale date,Amount,Currency,Total";

    private readonly DateTime _purchaseDate;
    private readonly decimal _amount;
    private readonly string _currency;
    private readonly DateTime _saleDate;
    private readonly string _saleTransactionID;
    private readonly string _purchaseTransactionID;

    public DateTime PurchaseDate => _purchaseDate;
    public decimal Amount => _amount;
    public string Currency => _currency;
    public decimal TotalAmount { get; set; }
    public string SaleTransactionID => _saleTransactionID;
    public string PurchaseTransactionID => _purchaseTransactionID;

    public TaxableEvent(string purchaseTransactionID, string saleTransactionID, DateTime purchaseDate, DateTime saleDate, decimal amount, string currency)
    {
        _amount = amount;
        _currency = currency;
        _saleDate = saleDate;
        _purchaseDate = purchaseDate;
        _saleTransactionID = saleTransactionID;
        _purchaseTransactionID = purchaseTransactionID;
    }

    public override string ToString()
    {
        return "\"" + _purchaseTransactionID + "\",\"" + _saleTransactionID + "\",\"" + _purchaseDate.ToString(CultureInfo.CurrentCulture) + "\",\"" + 
            _saleDate.ToString(CultureInfo.CurrentCulture) + "\",\"" + _amount.ToString() + "\",\"" + _currency + "\",\"" + TotalAmount.ToString() + "\"";
    }
}
