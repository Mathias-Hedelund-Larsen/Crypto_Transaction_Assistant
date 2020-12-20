using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public abstract class TransactionModelBase<T> : ITransactionModel where T : TransactionModelBase<T>
{
    public const string CSV_DATA_ORDER = "Transaction ID,Date,Crypto currency, Crypto currency amount,Native currency,Native currency amount,Transaction type,Wallet name";

    private string _transactionId;

    public DateTime TimeStamp { get; set; }

    public decimal NativeAmount { get; set; }

    public string NativeCurrency { get; set; }

    public string CryptoCurrency { get; set; }

    public decimal CryptoCurrencyAmount { get; set; }

    public decimal ValueForOneCryptoTokenInNative { get; set; }

    public TransactionType TransactionType { get; set; }

    public bool FullyTaxed { get; set; }

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

    protected async Task ConvertToNativeTarget(string nativeCurrency, decimal nativeAmount)
    {
        CurrencyConvertion currencyConvertion = MainComponent.Instance.CurrencyConvertionsContainer[nativeCurrency + "->" + MainComponent.Instance.TargetCurrency, TimeStamp];

        if (currencyConvertion != null)
        {
            NativeCurrency = MainComponent.Instance.TargetCurrency;
            NativeAmount = nativeAmount * currencyConvertion.ConvertionRate;
            ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
        }
        else
        {
            string date = TimeStamp.ToString("yyyy-MM-dd");

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    string exchangeRateText = await httpClient.GetStringAsync($"http://currencies.apps.grandtrunk.net/getrate/{date}/{nativeCurrency}/{MainComponent.Instance.TargetCurrency}");

                    if (decimal.TryParse(exchangeRateText, out decimal exchangeRate))
                    {
                        lock (MainComponent.Instance)
                        {
                            if (!MainComponent.Instance.CurrencyConvertionsContainer.ContainsKey(nativeCurrency + "->" + MainComponent.Instance.TargetCurrency, TimeStamp))
                            {
                                MainComponent.Instance.CurrencyConvertionsContainer.Add(new CurrencyConvertion(nativeCurrency, MainComponent.Instance.TargetCurrency, TimeStamp, exchangeRate));
                            }
                        }

                        NativeCurrency = MainComponent.Instance.TargetCurrency;
                        NativeAmount = nativeAmount * exchangeRate;
                        ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
                    }
                }
            }
            catch (Exception e)
            {
                //Write to screen too many transactions to get convertions for, please change ip or something, for api to let you get more data. Data is chached in files for later use.
                Debug.Log(e);
            }
        }
    }

    protected void AddUpdateCurrency()
    {       
        MainComponent.Instance.UpdateCurrencies += NativeCurrencyToTargetCurrency;
    }

    private async Task NativeCurrencyToTargetCurrency()
    {
        if (NativeCurrency != MainComponent.Instance.TargetCurrency)
        {
            await ConvertToNativeTarget(NativeCurrency, NativeAmount);
        }
        else
        {
            ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
        }
    }

    public override string ToString()
    {
        return TransactionId + "," + TimeStamp + "," + CryptoCurrency + "," + CryptoCurrencyAmount + "," + NativeCurrency + "," + NativeAmount + "," + TransactionType + "," + WalletName;
    }
}
