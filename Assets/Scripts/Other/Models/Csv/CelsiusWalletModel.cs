using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CelsiusWalletModel : TransactionModelBase
{
    private static readonly string[] FULL_TAX = new string[]
    {
        "interest",
        "referred_award"
    };

    private static readonly string[] NON_TAXABLE_EVENTS = new string[]
    {
        "withdrawal",
        "deposit"
    };

    public override string WalletName => "Celsius wallet";

    public override IEnumerable<TransactionModelBase> Init(string fileName, string[] entryData, Func<string, Type, object> convertFromString)
    {
        string transactionKind = entryData[2];

        if (NON_TAXABLE_EVENTS.Contains(transactionKind))
        {
            return Enumerable.Empty<TransactionModelBase>();
        }

        return InternalInit(transactionKind, entryData, convertFromString);
    }

    private IEnumerable<TransactionModelBase> InternalInit(string transactionKind, string[] entryData, Func<string, Type, object> convertFromString)
    {
        TimeStamp = (DateTime)convertFromString.Invoke(entryData[1], typeof(DateTime));
        
        CryptoCurrency = entryData[3];
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[4], typeof(decimal));

        NativeCurrency = "USD";
        NativeAmount = (decimal)convertFromString.Invoke(entryData[5], typeof(decimal));

        if (FULL_TAX.Contains(transactionKind))
        {
            TransactionType = TransactionType.Interest;
            FullyTaxed = true;
        }
        else
        {
            TransactionType = TransactionType.Purchase;
        }

        yield return this;

        UpdateCurrency();
    }

    public override TransactionModelBase Clone()
    {
        return (TransactionModelBase)MemberwiseClone();
    }
}
