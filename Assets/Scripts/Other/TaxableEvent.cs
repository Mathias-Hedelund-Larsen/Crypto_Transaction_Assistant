using System;

public sealed class TaxableEvent
{
    public const string CSV_DATA_ORDER = "Purchase transaction ID,Sale transaction ID,Crypto currency,Crypto currency sold amount,Purchase date,Sale date,Wallet,Amount,Fiat currency,Total";

    private readonly string _wallet;
    private readonly decimal _amount;
    private readonly string _currency;
    private readonly DateTime _saleDate;
    private readonly DateTime _purchaseDate;
    private readonly string _cryptoCurrency;
    private readonly string _saleTransactionID;
    private readonly string _purchaseTransactionID;
    private readonly decimal _cryptoCurrencyAmountSold;

    public DateTime PurchaseDate => _purchaseDate;
    public decimal Amount => _amount;
    public string Currency => _currency;
    public decimal TotalAmount { get; set; }
    public string SaleTransactionID => _saleTransactionID;
    public string PurchaseTransactionID => _purchaseTransactionID;

    public TaxableEvent(string purchaseTransactionID, string saleTransactionID, string cryptoCurrency, DateTime purchaseDate, DateTime saleDate, decimal amount, string currency, string wallet, decimal cryptoCurrencyAmountSold)
    {
        _amount = amount;
        _wallet = wallet;
        _currency = currency;
        _saleDate = saleDate;
        _purchaseDate = purchaseDate;
        _cryptoCurrency = cryptoCurrency;
        _saleTransactionID = saleTransactionID;
        _purchaseTransactionID = purchaseTransactionID;
        _cryptoCurrencyAmountSold = cryptoCurrencyAmountSold;
    }

    public override string ToString()
    {
        return "\"" + _purchaseTransactionID + "\",\"" + _saleTransactionID + "\",\"" + _cryptoCurrency + "\",\"" + _cryptoCurrencyAmountSold.ToString(MainComponent.Instance.CurrentCulture) + "\",\"" 
            + _purchaseDate.ToString(MainComponent.Instance.CurrentCulture) + "\",\"" +_saleDate.ToString(MainComponent.Instance.CurrentCulture) + "\",\"" 
            + _wallet + "\",\"" + _amount.ToString(MainComponent.Instance.CurrentCulture) + "\",\"" + _currency + 
            "\",\"" + TotalAmount.ToString(MainComponent.Instance.CurrentCulture) + "\"";
    }

    public int CompareTo(TaxableEvent other)
    {
        int date = DateTime.Compare(_purchaseDate, other._purchaseDate);

        if(date == 0)
        {
            return DateTime.Compare(_saleDate, other._saleDate);
        }

        return date;
    }
}
