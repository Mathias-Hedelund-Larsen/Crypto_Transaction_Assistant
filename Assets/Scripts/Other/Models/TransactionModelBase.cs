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
        if (CryptoCurrencyAmount != 0)
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

                using (CallRateLimiter httpClient = new CallRateLimiter("Grandtrunk-Currencies", 5, TimeSpan.FromSeconds(1)))
                {
                    HttpResponseMessage httpResponse = await httpClient.GetDataAsync($"http://currencies.apps.grandtrunk.net/getrate/{date}/{nativeCurrency}/{MainComponent.Instance.TargetCurrency}");

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (decimal.TryParse(await httpResponse.Content.ReadAsStringAsync(), out decimal exchangeRate))
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
                    else
                    {

                    }
                }
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
            if (CryptoCurrencyAmount != 0)
            {
                ValueForOneCryptoTokenInNative = NativeAmount / CryptoCurrencyAmount;
            }
        }
    }

    public override string ToString()
    {
        return TransactionId + "," + TimeStamp + "," + CryptoCurrency + "," + CryptoCurrencyAmount + "," + NativeCurrency + "," + NativeAmount + "," + TransactionType + "," + WalletName;
    }

    public int CompareTo(ITransactionModel other)
    {
        int date = DateTime.Compare(TimeStamp, other.TimeStamp);

        if(date == 0)
        {
            if(TransactionType == TransactionType.Purchase)
            {
                return -1;
            }

            return 1;
        }

        return date;
    }
}
