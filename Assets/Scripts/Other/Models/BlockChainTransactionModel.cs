using CoingeckoPriceData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public sealed class BlockChainTransactionModel : TransactionModelBase<BlockChainTransactionModel>
{
    private static readonly string API_KEYS_PATH = CryptoService.TRANSACTIONS_FOLDER_PATH + "/BlockChainSpecific/APIKeys.json";

    private readonly static string[] FULL_TAX = new string[]
    {
        "Atomic Wallet Staking Rewards Distribution"
    };

    private string _address;
    private string _transactionId;

    public override string WalletName => "No Wallet Indicated";

    private BlockChainTransactionModel()
    {

    }

    public static async Task<List<ITransactionModel>> InitFromData(string fileName, string entryData, Func<string, Type, object> convertFromString)
    {
        TransactionTracking transactionTracking = JsonConvert.DeserializeObject<TransactionTracking>(entryData);

        foreach (AddressInfo addressInfo in transactionTracking.TransactionHistory)
        {
            foreach (TransactionIdAndType markedTransaction in addressInfo.MarkedTransactions)
            {
                //value.TransactionType
            }
        }

        BlockChainTransactionModel blockChainTransactionModel = new BlockChainTransactionModel();

        return new List<ITransactionModel>();
    }

    /// <summary>
    /// Set the _transactionId, _address, _blockChainToCall and the TimeStamp before calling this part of the initialization.
    /// </summary>
    /// <param name="coinId"></param>
    /// <returns></returns>
    private async Task<List<ITransactionModel>> Init(string coinId)
    {
        string date = TimeStamp.ToString("dd-MM-yyyy");

        using (CallRateLimiter apiCallRateLimiter = new CallRateLimiter("CoinGeckoPrice", 10, TimeSpan.FromSeconds(1)))
        {
            HttpResponseMessage httpResponse = await apiCallRateLimiter.GetDataAsync($"http://api.coingecko.com/api/v3/coins/{coinId}/history?date={date}&localization=false");

            if (httpResponse.IsSuccessStatusCode)
            {
                CoinInformation coinData = JsonConvert.DeserializeObject<CoinInformation>(await httpResponse.Content.ReadAsStringAsync());

                if (coinData.Market_data.Current_price.ContainsKey(MainComponent.Instance.TargetCurrency.ToLower()))
                {
                    NativeCurrency = MainComponent.Instance.TargetCurrency;
                    ValueForOneCryptoTokenInNative = coinData.Market_data.Current_price[MainComponent.Instance.TargetCurrency.ToLower()];

                    //switch (_blockChainToCall)
                    //{
                    //    case BlockChain.Binance:
                    //        return await InitFromBinanceApi();

                    //    case BlockChain.Ethereum:
                    //        return await InitFromEthereumApi();

                    //}
                }
            }
            else
            {
                Debug.Log("Coingecko Call failed with code: " + httpResponse.StatusCode + " and message: " + await httpResponse.Content.ReadAsStringAsync());
            }
        }

        return new List<ITransactionModel>();
    }

    private async Task<List<ITransactionModel>> InitFromBinanceApi()
    {
        List<ITransactionModel> transactionModels = new List<ITransactionModel> { this };

        using (CallRateLimiter apiCallRateLimiter = new CallRateLimiter("BinanceApiCall", 10, TimeSpan.FromSeconds(1)))
        {
            HttpResponseMessage httpResponse = await apiCallRateLimiter.GetDataAsync($"https://dex.binance.org/api/v1/tx/{_transactionId}?format=json");

            if (httpResponse.IsSuccessStatusCode)
            {
                BinanceChain.TransactionFromHash transactionFromHash = JsonConvert.DeserializeObject<BinanceChain.TransactionFromHash>(await httpResponse.Content.ReadAsStringAsync());

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
                                    blockChainTransactionModel.NativeAmount += ValueForOneCryptoTokenInNative * coin.Amount;
                                    blockChainTransactionModel.CryptoCurrency += coin.Denom + "|";
                                }

                                blockChainTransactionModel.CryptoCurrency.Remove(blockChainTransactionModel.CryptoCurrency.Length - 1);
                                transactionModels.Add(blockChainTransactionModel);
                            }

                            BinanceChain.TransactionAddressAndCoins first = transactionInformationMessageValues[0];

                            NativeAmount = 0;

                            foreach (BinanceChain.TransactionCoin coin in first.Coins)
                            {
                                CryptoCurrency = coin.Denom;
                                CryptoCurrencyAmount += coin.Amount;
                                NativeAmount += ValueForOneCryptoTokenInNative * coin.Amount;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Binance api call failed with code: " + httpResponse.StatusCode + " and message " + await httpResponse.Content.ReadAsStringAsync());
            }
        }

        return transactionModels;
    }

    private async Task<List<ITransactionModel>> InitFromEthereumApi()
    {
        List<ITransactionModel> transactionModels = new List<ITransactionModel> { this };

        APIKeys keys;

        using (StreamReader streamReader = new StreamReader(API_KEYS_PATH))
        {
            string apiKeys = await streamReader.ReadToEndAsync();

            keys = JsonConvert.DeserializeObject<APIKeys>(apiKeys);
        }

        using (CallRateLimiter apiCallRateLimiter = new CallRateLimiter("EtherScan", 5, TimeSpan.FromSeconds(1)))
        {
            HttpResponseMessage etherTransactionsResponse = await apiCallRateLimiter.GetDataAsync($"https://api.etherscan.io/api?module=account&action=txlist&address={_address}&sort=asc&apikey={keys.EtherScanKey}");
            HttpResponseMessage erc20TransactionsResponse = await apiCallRateLimiter.GetDataAsync($"https://api.etherscan.io/api?module=account&action=tokentx&address={_address}&sort=asc&apikey={keys.EtherScanKey}");

            if (etherTransactionsResponse.IsSuccessStatusCode && erc20TransactionsResponse.IsSuccessStatusCode)
            {
                EthereumChain.EtherAddressTransactions etherAddressTransactions = JsonConvert.DeserializeObject<EthereumChain.EtherAddressTransactions>(await etherTransactionsResponse.Content.ReadAsStringAsync());
                EthereumChain.Erc20AddressTransactions erc20AddressTransactions = JsonConvert.DeserializeObject<EthereumChain.Erc20AddressTransactions>(await erc20TransactionsResponse.Content.ReadAsStringAsync());

                List<EthereumChain.IEtherScanTransaction> transactions = new List<EthereumChain.IEtherScanTransaction>();

                transactions.AddRange(erc20AddressTransactions.Result);

                foreach (var ethTransaction in etherAddressTransactions.Result)
                {
                    if (!transactions.Any(t => t.Hash == ethTransaction.Hash))
                    {
                        transactions.Add(ethTransaction);
                    }
                }

                if (transactions.Count > 0)
                {
                    for (int i = 1; i < transactions.Count; i++)
                    {
                        BlockChainTransactionModel blockChainTransactionModel = Clone();

                        NativeAmount = transactions[i].Value * ValueForOneCryptoTokenInNative;
                        CryptoCurrency = transactions[i].TokenSymbol;
                        CryptoCurrencyAmount = transactions[i].Value;
                    }

                    EthereumChain.IEtherScanTransaction first = transactions[0];
                    NativeAmount = first.Value * ValueForOneCryptoTokenInNative;
                    CryptoCurrency = first.TokenSymbol;
                    CryptoCurrencyAmount = first.Value;
                }
            }
            else
            {
                Debug.Log("Etherscan api call finished with code: " + etherTransactionsResponse.StatusCode + " and message " + await etherTransactionsResponse.Content.ReadAsStringAsync());
                Debug.Log("Etherscan api call finished with code: " + erc20TransactionsResponse.StatusCode + " and message " + await erc20TransactionsResponse.Content.ReadAsStringAsync());
            }
        }

        return transactionModels;
    }

    private BlockChainTransactionModel Clone()
    {
        return (BlockChainTransactionModel)MemberwiseClone();
    }
}
