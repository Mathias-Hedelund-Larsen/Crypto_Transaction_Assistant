using CoingeckoPriceData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class BlockChainTransactionModel : TransactionModelBase<BlockChainTransactionModel>
{
    private static readonly string API_KEYS_PATH = CryptoService.TRANSACTIONS_FOLDER_PATH + "/BlockChainSpecific/APIKeys.json";

    protected const string BINANCE_INDICATOR = "explorer.binance.org";
    protected const string ETHEREUM_INDICATOR = "etherscan.io/tx/";


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

    public static async Task<List<ITransactionModel>> InitFromData(string fileName, string entryData, Func<string, Type, object> convertFromString)
    {
        TransactionTracking transactionTracking = JsonConvert.DeserializeObject<TransactionTracking>(entryData);

        BlockChainTransactionModel blockChainTransactionModel = new BlockChainTransactionModel();

        return new List<ITransactionModel>();
    }

    /// <summary>
    /// Set the _transactionId, _address, _blockChainToCall and the TimeStamp before calling this part of the initialization.
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

                        case BlockChain.Ethereum:
                            return await InitFromEthereumApi();

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

        using (HttpClient transactionData = new HttpClient())
        {
            string etherTransactions = await transactionData.GetStringAsync($"https://api.etherscan.io/api?module=account&action=txlist&address={_address}&sort=asc&apikey={keys.EtherScanKey}");
            string erc20Transactions = await transactionData.GetStringAsync($"https://api.etherscan.io/api?module=account&action=tokentx&address={_address}&sort=asc&apikey={keys.EtherScanKey}");

            EthereumChain.EtherAddressTransactions etherAddressTransactions = JsonConvert.DeserializeObject<EthereumChain.EtherAddressTransactions>(etherTransactions);
            EthereumChain.Erc20AddressTransactions erc20AddressTransactions = JsonConvert.DeserializeObject<EthereumChain.Erc20AddressTransactions>(erc20Transactions);

            List<EthereumChain.IEtherScanTransaction> transactions = new List<EthereumChain.IEtherScanTransaction>();

            transactions.AddRange(erc20AddressTransactions.Result);

            foreach (var ethTransaction in etherAddressTransactions.Result)
            {
                if(!transactions.Any(t => t.Hash == ethTransaction.Hash))
                {
                    transactions.Add(ethTransaction);
                }
            }

            if(transactions.Count > 0)
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

        return transactionModels;
    }

    private BlockChainTransactionModel Clone()
    {
        return (BlockChainTransactionModel)MemberwiseClone();
    }
}
