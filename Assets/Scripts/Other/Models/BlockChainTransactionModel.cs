using CoingeckoPriceData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BlockChainTransactionModel : TransactionModelBase<BlockChainTransactionModel>
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

    public override string WalletName => "No Wallet Indicated";

    protected BlockChainTransactionModel()
    {

    }

    /// <summary>
    /// Set the CryptoCurrency, CryptoCurrencyAmount, _transactionId, _address, _blockChainToCall and the TimeStamp before calling this part of the initialization.
    /// </summary>
    /// <param name="coinId"></param>
    /// <returns></returns>
    protected async Task<List<ITransactionModel>> Init(string coinId)
    {
        string date = TimeStamp.ToString("dd-MM-yyyy");
        try
        {
            using (HttpClient historicalDataWebRequest = new HttpClient())
            {
                string responseText = await historicalDataWebRequest.GetStringAsync($"http://api.coingecko.com/api/v3/coins/{coinId}/history?date={date}&localization=false");

                CoinInformation coinData = JsonConvert.DeserializeObject<CoinInformation>(responseText);

                if (coinData.Market_data.Current_price.ContainsKey(MainComponent.Instance.TargetCurrency.ToLower()))
                {
                    NativeCurrency = MainComponent.Instance.TargetCurrency;
                    ValueForOneCryptoTokenInNative = coinData.Market_data.Current_price[MainComponent.Instance.TargetCurrency.ToLower()];

                    switch (_blockChainToCall)
                    {
                        case BlockChain.Binance:
                            return await InitFromBinanceApi();

                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return new List<ITransactionModel>();
    }

    private async Task<List<ITransactionModel>> InitFromBinanceApi()
    {
        List<ITransactionModel> transactionModels = new List<ITransactionModel> { this };

        using (HttpClient transactionData = new HttpClient())
        {
            string responseText = await transactionData.GetStringAsync($"https://dex.binance.org/api/v1/tx/{_transactionId}?format=json");

            BinanceChain.TransactionFromHash transactionFromHash = JsonConvert.DeserializeObject<BinanceChain.TransactionFromHash>(responseText);

            if (transactionFromHash.Ok)
            {
                if (FULL_TAX.Contains(transactionFromHash.Tx.Value.Memo))
                {
                    FullyTaxed = true;
                }

                foreach (BinanceChain.TransactionInformationMessage transactionInformationMessage in transactionFromHash.Tx.Value.Msg)
                {
                    List<BinanceChain.TransactionAddressAndCoins> transactionInformationMessageValues = transactionInformationMessage.Value.Outputs.Where(b => b.Address == _address).ToList();

                    if (transactionInformationMessageValues.Count > 0)
                    {
                        for (int i = 1; i < transactionInformationMessageValues.Count; i++)
                        {
                            BlockChainTransactionModel blockChainTransactionModel = Clone();

                            foreach (BinanceChain.TransactionCoin coin in transactionInformationMessageValues[i].Coins)
                            {
                                NativeAmount += ValueForOneCryptoTokenInNative * coin.Amount;
                            }
                        }

                        BinanceChain.TransactionAddressAndCoins first = transactionInformationMessageValues[0];

                        NativeAmount = 0;

                        foreach (BinanceChain.TransactionCoin coin in first.Coins)
                        {
                            NativeAmount += ValueForOneCryptoTokenInNative * coin.Amount;
                        }                     
                    }
                }

            }
        }

        return transactionModels;
    }

    private BlockChainTransactionModel Clone()
    {
        return (BlockChainTransactionModel)MemberwiseClone();
    }
}
