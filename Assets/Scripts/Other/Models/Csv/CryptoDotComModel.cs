using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CryptoDotComModel : TransactionModelBase
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

    public override IEnumerable<TransactionModelBase> Init(string fileName, string[] entryData, Func<string, Type, object> convertFromString)
    {       
        string transactionKind = entryData[9];

        if (NON_TAXABLE_EVENTS.Contains(transactionKind))
        {
            return Enumerable.Empty<TransactionModelBase>();
        }

        return InternalInit(transactionKind, entryData, convertFromString);
    }

    private IEnumerable<TransactionModelBase> InternalInit(string transactionKind, string[] entryData, Func<string, Type, object> convertFromString)
    {
        TimeStamp = (DateTime)convertFromString.Invoke(entryData[0], typeof(DateTime));

        if (transactionKind == VIBAN_PURCHASE)
        {
            CryptoCurrency = entryData[4];
            CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[5], typeof(decimal));
        }
        else
        {
            CryptoCurrency = entryData[2];
            CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[3], typeof(decimal));
        }

        NativeCurrency = entryData[6];
        NativeAmount = (decimal)convertFromString.Invoke(entryData[7], typeof(decimal));

        yield return this;

        if (REVERSION.Contains(transactionKind))
        {
            TransactionType = TransactionType.Reversion;
        }
        else if (transactionKind == CRYPTO_EXHANGE)
        {
            TransactionType = TransactionType.Sale;

            yield return SetupExchangeTransaction(entryData, convertFromString);
        }
        else if (SALES.Contains(transactionKind))
        {
            TransactionType = TransactionType.Sale;
        }
        else
        {
            TransactionType = TransactionType.Purchase;

            if (REBATES.Contains(transactionKind))
            {
                TransactionType = TransactionType.Rebate;
            }

            if (FULL_TAX.Contains(transactionKind))
            {
                if (transactionKind == FULL_TAX[0])
                {
                    TransactionType = TransactionType.Interest;
                }

                FullyTaxed = true;
            }
        }

        UpdateCurrency();
    }

    private CryptoDotComModel SetupExchangeTransaction(string[] entryData, Func<string, Type, object> convertFromString)
    {
        TransactionType = TransactionType.Sale;
        CryptoCurrencyAmount *= -1;

        CryptoDotComModel next = new CryptoDotComModel();

        string[] nEntryData = new string[] { entryData[4], entryData[5] };

        next.InternalEnumerableInit(TimeStamp, NativeAmount, NativeCurrency, nEntryData, convertFromString);

        return next;
    }

    private void InternalEnumerableInit(DateTime timeStamp, decimal nativeAmount, string nativeCurrency, string[] entryData, Func<string, Type, object> convertFromString)
    {
        TransactionType = TransactionType.Purchase;
        TimeStamp = timeStamp;
        NativeAmount = nativeAmount;
        NativeCurrency = nativeCurrency;
        CryptoCurrency = entryData[0];
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[1], typeof(decimal));

        UpdateCurrency();
    }

    public override TransactionModelBase Clone()
    {
        return (CryptoDotComModel)MemberwiseClone();
    }
}
