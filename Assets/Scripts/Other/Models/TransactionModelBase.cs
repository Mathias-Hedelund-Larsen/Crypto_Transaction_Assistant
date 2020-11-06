using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public abstract class TransactionModelBase
{
    public const string CSV_DATA_ORDER = "Transaction ID,Date,Crypto currency, Crypto currency amount,Native currency,Native currency amount,Transaction type";

    private string _transactionId;

    public DateTime TimeStamp { get; protected set; }

    public bool IsAwatingData { get; protected set; }

    public decimal NativeAmount { get; protected set; }

    public string NativeCurrency { get; protected set; }

    public string CryptoCurrency { get; protected set; }

    public decimal CryptoCurrencyAmount { get; set; }

    public decimal ValueForOneCryptoTokenInNative { get; protected set; }

    public TransactionType TransactionType { get; protected set; }

    public abstract string WalletName { get; }

    public string TransactionId 
    { 
        get
        {
            if (string.IsNullOrWhiteSpace(_transactionId))
            {
                _transactionId = Guid.NewGuid().ToString();
            }

            return _transactionId;
        }
    }

    public abstract IEnumerable<TransactionModelBase> Init(string[] entryData, Func<string, Type, object> convertFromString);

    protected IEnumerator ConvertToNativeTarget(string nativeCurrency, decimal nativeAmount)
    {
        CurrencyConvertion currencyConvertion = MainComponent.Instance.CurrencyConvertionsContainer[nativeCurrency + "->" + MainComponent.Instance.TargetCurrency, TimeStamp];

        if (currencyConvertion != null)
        {
            NativeCurrency = MainComponent.Instance.TargetCurrency;
            NativeAmount = nativeAmount * currencyConvertion.ConvertionRate;
            ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
            IsAwatingData = false;
        }
        else
        {
            string date = TimeStamp.ToString("yyyy-MM-dd");

            using (UnityWebRequest exhangeWebRequest = UnityWebRequest.Get($"http://currencies.apps.grandtrunk.net/getrate/{date}/{nativeCurrency}/{MainComponent.Instance.TargetCurrency}"))
            {
                yield return exhangeWebRequest.SendWebRequest();

                if (exhangeWebRequest.isNetworkError)
                {
                }
                else
                {
                    if (decimal.TryParse(exhangeWebRequest.downloadHandler.text, out decimal exchangeRate))
                    {
                        MainComponent.Instance.CurrencyConvertionsContainer.Add(new CurrencyConvertion(nativeCurrency, MainComponent.Instance.TargetCurrency, TimeStamp, exchangeRate));

                        NativeCurrency = MainComponent.Instance.TargetCurrency;
                        NativeAmount = nativeAmount * exchangeRate;
                        ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
                        IsAwatingData = false;
                    }
                }
            }
        }
    }

    public abstract TransactionModelBase Clone();

    public override string ToString()
    {
        return TransactionId + "," + TimeStamp + "," + CryptoCurrency + "," + CryptoCurrencyAmount + "," + NativeCurrency + "," + NativeAmount + "," + TransactionType;
    }
}
