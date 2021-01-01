using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    private CelsiusWalletModel() {}

    public static async Task<List<ITransactionModel>> InitializeFromData(string fileName, string entryData, Func<string, Type, object> convertFromString)
    {
        string[] dataSplit = entryData.Split(',');

        string transactionKind = dataSplit[2];

        if (NON_TAXABLE_EVENTS.Contains(transactionKind))
        {
            return new List<ITransactionModel>();
        }

        return await InternalInit(transactionKind, dataSplit, convertFromString);
    }

    private static async Task<List<ITransactionModel>> InternalInit(string transactionKind, string[] entryData, Func<string, Type, object> convertFromString)
    {
        CelsiusWalletModel celsiusWalletModel = new CelsiusWalletModel();

        celsiusWalletModel.TimeStamp = (DateTime)convertFromString.Invoke(entryData[1], typeof(DateTime));

        celsiusWalletModel.CryptoCurrency = entryData[3];
        celsiusWalletModel.CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[4], typeof(decimal));

        celsiusWalletModel.NativeCurrency = "USD";
        celsiusWalletModel.NativeAmount = (decimal)convertFromString.Invoke(entryData[5], typeof(decimal));

        if (FULL_TAX.Contains(transactionKind))
        {
            celsiusWalletModel.TransactionType = TransactionType.Interest;
            celsiusWalletModel.IsFullyTaxed = true;
        }
        else
        {
            celsiusWalletModel.TransactionType = TransactionType.Purchase;
        }

        celsiusWalletModel.AddUpdateCurrency();

        return new List<ITransactionModel> { celsiusWalletModel };
    }

    public override ITransactionModel Clone()
    {
        return (CelsiusWalletModel)MemberwiseClone();
    }
}
