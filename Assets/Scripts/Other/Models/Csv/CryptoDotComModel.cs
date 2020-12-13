using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class CryptoDotComModel : TransactionModelBase<CryptoDotComModel>
{
    private const string VIBAN_PURCHASE = "viban_purchase";

    private const string CRYPTO_EXHANGE = "crypto_exchange";

    private static readonly string[] REBATES =
    {
        "referral_card_cashback",
        "reimbursement"
    };

    private static readonly string[] SALES = new string[]
    {
        "card_top_up",
        "dust_conversion_debited"
    };

    private static readonly string[] REVERSION = new string[]
    {
        "reimbursement_reverted",
        "card_cashback_reverted"
    };

    private static readonly string[] NON_TAXABLE_EVENTS = new string[] 
    {
        "lockup_lock",
        "crypto_withdrawal",
        "crypto_deposit",
        "supercharger_deposit",
        "supercharger_withdrawal",
        "crypto_to_exchange_transfer",
        "exchange_to_crypto_transfer",
        "lockup_upgrade"
    };

    private static readonly string[] FULL_TAX = new string[]
    {
        "crypto_earn_interest_paid",
        "mco_stake_reward",
        "referral_gift",
        "referral_bonus"
    };

    public override string WalletName => "CryptoDotCom";

    private CryptoDotComModel() {}

    public static async Task<List<ITransactionModel>> InitializeFromData(string fileName, string entryData, Func<string, Type, object> convertFromString)
    {
        string[] dataSplit = entryData.Split(',');

        string transactionKind = dataSplit[9];

        if (NON_TAXABLE_EVENTS.Contains(transactionKind))
        {
            return new List<ITransactionModel>();
        }

        return await InitializeFromData(transactionKind, dataSplit, convertFromString);
    }

    private static async Task<List<ITransactionModel>> InitializeFromData(string transactionKind, string[] entryData, Func<string, Type, object> convertFromString)
    {
        List<ITransactionModel> transactions = new List<ITransactionModel>();

        CryptoDotComModel cryptoDotComModel = new CryptoDotComModel();

        cryptoDotComModel.TimeStamp = (DateTime)convertFromString.Invoke(entryData[0], typeof(DateTime));

        if (transactionKind == VIBAN_PURCHASE)
        {
            cryptoDotComModel.CryptoCurrency = entryData[4];
            cryptoDotComModel.CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[5], typeof(decimal));
        }
        else
        {
            cryptoDotComModel.CryptoCurrency = entryData[2];
            cryptoDotComModel.CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[3], typeof(decimal));
        }

        cryptoDotComModel.NativeCurrency = entryData[6];
        cryptoDotComModel.NativeAmount = (decimal)convertFromString.Invoke(entryData[7], typeof(decimal));

        transactions.Add(cryptoDotComModel);

        if (REVERSION.Contains(transactionKind))
        {
            cryptoDotComModel.TransactionType = TransactionType.Reversion;
        }
        else if (transactionKind == CRYPTO_EXHANGE)
        {
            cryptoDotComModel.TransactionType = TransactionType.Sale;

            transactions.Add(await SetupExchangeTransaction(cryptoDotComModel, entryData, convertFromString));
        }
        else if (SALES.Contains(transactionKind))
        {
            cryptoDotComModel.TransactionType = TransactionType.Sale;
        }
        else
        {
            cryptoDotComModel.TransactionType = TransactionType.Purchase;

            if (REBATES.Contains(transactionKind))
            {
                cryptoDotComModel.TransactionType = TransactionType.Rebate;
            }

            if (FULL_TAX.Contains(transactionKind))
            {
                if (transactionKind == FULL_TAX[0])
                {
                    cryptoDotComModel.TransactionType = TransactionType.Interest;
                }

                cryptoDotComModel.FullyTaxed = true;
            }
        }

        await cryptoDotComModel.UpdateCurrency();

        return transactions;
    }

    private static async Task<ITransactionModel> SetupExchangeTransaction(ITransactionModel original, string[] entryData, Func<string, Type, object> convertFromString)
    {
        original.TransactionType = TransactionType.Sale;
        original.CryptoCurrencyAmount *= -1;

        CryptoDotComModel next = new CryptoDotComModel();

        string[] nEntryData = new string[] { entryData[4], entryData[5] };

        await next.InternalInit(original.TimeStamp, original.NativeAmount, original.NativeCurrency, nEntryData, convertFromString);

        return next;
    }

    private async Task InternalInit(DateTime timeStamp, decimal nativeAmount, string nativeCurrency, string[] entryData, Func<string, Type, object> convertFromString)
    {
        TransactionType = TransactionType.Purchase;
        TimeStamp = timeStamp;
        NativeAmount = nativeAmount;
        NativeCurrency = nativeCurrency;
        CryptoCurrency = entryData[0];
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[1], typeof(decimal));

        await UpdateCurrency();
    }
}
