using System;
using System.Collections.Generic;

public sealed class CryptoDotComModel : TransactionModelBase
{
    private const string CRYPTO_EXHANGE = "crypto_exchange";
    private const string CRYPTO_PURCHASE = "crypto_purchase";

    public override string WalletName => "CryptoDotCom";

    public override IEnumerable<TransactionModelBase> Init(string[] entryData, Func<string, Type, object> convertFromString)
    {
        string transactionType = entryData[9];

        TimeStamp = (DateTime)convertFromString.Invoke(entryData[0], typeof(DateTime));

        CryptoCurrency = entryData[2];
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[3], typeof(decimal));

        NativeCurrency = entryData[6];
        NativeAmount = (decimal)convertFromString.Invoke(entryData[7], typeof(decimal));

        yield return this;

        if (transactionType == CRYPTO_EXHANGE)
        {
            TransactionType = TransactionType.Sale;

            yield return SetupExchangeTransaction(entryData, convertFromString);
        }
        else
        {
            TransactionType = TransactionType.Purchase;
        }

        if (NativeCurrency != MainComponent.Instance.TargetCurrency)
        {
            IsAwatingData = true;
            MainComponent.Instance.StartCoroutine(ConvertToNativeTarget(NativeCurrency, NativeAmount));
        }
        else
        {
            IsAwatingData = false;
            ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
        }
    }

    private CryptoDotComModel SetupExchangeTransaction(string[] entryData, Func<string, Type, object> convertFromString)
    {
        TransactionType = TransactionType.Sale;
        CryptoCurrencyAmount *= -1;

        CryptoDotComModel next = new CryptoDotComModel();

        string[] nEntryData = new string[] { entryData[4], entryData[5] };

        next.Init(TimeStamp, NativeAmount, NativeCurrency, nEntryData, convertFromString);

        return next;
    }

    private void Init(DateTime timeStamp, decimal nativeAmount, string nativeCurrency, string[] entryData, Func<string, Type, object> convertFromString)
    {
        TransactionType = TransactionType.Purchase;
        TimeStamp = timeStamp;
        NativeAmount = nativeAmount;
        NativeCurrency = nativeCurrency;
        CryptoCurrency = entryData[0];
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[1], typeof(decimal));

        if (NativeCurrency != MainComponent.Instance.TargetCurrency)
        {
            IsAwatingData = true;
            MainComponent.Instance.StartCoroutine(ConvertToNativeTarget(NativeCurrency, NativeAmount));
        }
        else
        {
            IsAwatingData = false;
            ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
        }
    }

    public override TransactionModelBase Clone()
    {
        return (CryptoDotComModel)MemberwiseClone();
    }
}
