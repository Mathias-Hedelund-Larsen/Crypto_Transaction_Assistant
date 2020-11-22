using CoingeckoPriceData;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BlockChainTransactionModelBase : TransactionModelBase
{
    protected const string BINANCE_INDICATOR = "explorer.binance.org";

    public enum BlockChain
    {
        Binance = 0
    }

    private readonly static string[] FULL_TAX = new string[]
    {
        "Atomic Wallet Staking Rewards Distribution"
    };

    protected string _address;
    protected string _transactionId;

    /// <summary>
    /// This field needs to be set before calling Init
    /// </summary>
    protected BlockChain _blockChainToCall;

    /// <summary>
    /// Set the CryptoCurrency, CryptoCurrencyAmount, _transactionId, _address, _blockChainToCall and the TimeStamp before calling this part of the initialization.
    /// </summary>
    /// <param name="coinId"></param>
    /// <returns></returns>
    protected IEnumerator Init(string coinId)
    {
        IsAwatingData = true;

        string date = TimeStamp.ToString("dd-MM-yyyy");

        using (UnityWebRequest historicalDataWebRequest = UnityWebRequest.Get($"api.coingecko.com/api/v3/coins/{coinId}/history?date={date}&localization=false"))
        {
            yield return historicalDataWebRequest.SendWebRequest();

            if (historicalDataWebRequest.isNetworkError)
            {
            }
            else
            {
                CoinInformation coinData = JsonConvert.DeserializeObject<CoinInformation>(historicalDataWebRequest.downloadHandler.text);

                if (coinData.Market_data.Current_price.ContainsKey(MainComponent.Instance.TargetCurrency.ToLower()))
                {
                    NativeCurrency = MainComponent.Instance.TargetCurrency;
                    ValueForOneCryptoTokenInNative = coinData.Market_data.Current_price[MainComponent.Instance.TargetCurrency.ToLower()];

                    NativeAmount = ValueForOneCryptoTokenInNative * CryptoCurrencyAmount;

                    switch (_blockChainToCall)
                    {
                        case BlockChain.Binance:
                            MainComponent.Instance.StartCoroutine(CallBinanceApi());
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set the _address and the _transactionId before calling this method
    /// </summary>
    /// <returns></returns>
    protected IEnumerator CallBinanceApi()
    {
        using (UnityWebRequest transactionData = UnityWebRequest.Get($"https://dex.binance.org/api/v1/tx/{_transactionId}?format=json"))
        {
            yield return transactionData.SendWebRequest();

            if (transactionData.isNetworkError)
            {
            }
            else
            {
                BinanceChain.TransactionFromHash transactionFromHash = JsonUtility.FromJson<BinanceChain.TransactionFromHash>(transactionData.downloadHandler.text);

                if (transactionFromHash.Ok)
                {
                    if (FULL_TAX.Contains(transactionFromHash.Tx.Value.Memo))
                    {
                        FullyTaxed = true;

                        IsAwatingData = false;
                    }

                    foreach (BinanceChain.TransactionInformationMessage transactionInformationMessage in transactionFromHash.Tx.Value.Msg)
                    {
                        BinanceChain.TransactionAddressAndCoins transactionInformationMessageValues = transactionInformationMessage.Value.Outputs.FirstOrDefault(b => b.Address == _address);

                        if (transactionInformationMessage != null)
                        {

                        }
                    }
                }
            }
        }
    }
}
