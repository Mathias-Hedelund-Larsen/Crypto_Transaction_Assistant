using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class CryptoService : ICryptoService
{
    public readonly static string TRANSACTIONS_FOLDER_PATH = Application.persistentDataPath + "/Crypto_Transactions";
    public readonly static string CURRENCY_CONVERTIONS_PATH = Application.persistentDataPath + "/CryptoApplicationData/CurrencyConvertions.Json";
    public readonly static string CURRENCY_CODES_FILE_PATH = Application.persistentDataPath + "/CryptoApplicationData/CurrencyCodes.txt";
    public readonly static string TRANSACTIONS_TRACKING_PATH = TRANSACTIONS_FOLDER_PATH + "/TransactionsTracking.Json";

    private readonly static string[] SUPPORTED_TRANSACTIONS_FORMAT_PATHS = new string[]
    {
        TRANSACTIONS_FOLDER_PATH + "/CryptoDotCom",
        TRANSACTIONS_FOLDER_PATH + "/Celsius",
        TRANSACTIONS_FOLDER_PATH + "/CoinBase"
    };

    private List<TaxableEvent> _gains = new List<TaxableEvent>();
    private List<TaxableEvent> _losses = new List<TaxableEvent>();
    private Dictionary<string, List<ITransactionModel>> _transactions = new Dictionary<string, List<ITransactionModel>>();
    private readonly Dictionary<Type, Func<string, string, Func<string, Type, object>, Task<List<ITransactionModel>>>> _createTransactions;

    public CryptoService()
    {
        _createTransactions = new Dictionary<Type, Func<string, string, Func<string, Type, object>, Task<List<ITransactionModel>>>>();

        _createTransactions.Add(typeof(CryptoDotComModel), CryptoDotComModel.InitializeFromData);
        _createTransactions.Add(typeof(CelsiusWalletModel), CelsiusWalletModel.InitializeFromData);
        _createTransactions.Add(typeof(BlockChainTransactionModel), BlockChainTransactionModel.InitFromData);

        GenerateFolders();

        GenerateReadMeInFolders();

        List<Task<IEnumerable<ITransactionModel>>> transactionModelTasks = new List<Task<IEnumerable<ITransactionModel>>>();

        List<TransactionInformation> transactionInformations = new List<TransactionInformation>();

        foreach (string directoryPath in SUPPORTED_TRANSACTIONS_FORMAT_PATHS)
        {
            string[] filePaths = Directory.GetFiles(directoryPath).Where(f => !f.ToLower().Contains("readme.txt")).ToArray();

            Type modelType = null;

            if (directoryPath == SUPPORTED_TRANSACTIONS_FORMAT_PATHS[0])
            {
                modelType = typeof(CryptoDotComModel);
            }
            else if (directoryPath == SUPPORTED_TRANSACTIONS_FORMAT_PATHS[1])
            {
                modelType = typeof(CelsiusWalletModel);
            }
            else if (directoryPath == SUPPORTED_TRANSACTIONS_FORMAT_PATHS[2])
            {
            }

            foreach (string filePath in filePaths)
            {
                IEnumerable<string> lines = File.ReadAllLines(filePath).Skip(1);

                foreach (string line in lines)
                {
                    transactionInformations.Add(new TransactionInformation(Path.GetFileName(filePath), line, modelType));                    
                }
            }
        }

        Task.Run(() => InitializeTransaction(transactionInformations)).Wait();
    }

    private void GenerateReadMeInFolders()
    {
        GenerateAtomicWalletReadme();
    }

    private void GenerateAtomicWalletReadme()
    {
        if (!File.Exists(SUPPORTED_TRANSACTIONS_FORMAT_PATHS[2]))
        {
            using (StreamWriter writer = new StreamWriter(SUPPORTED_TRANSACTIONS_FORMAT_PATHS[2] + "/readme.txt"))
            {
                writer.WriteLine("Youtube link: ");
                writer.WriteLine("Move transaction csv files generated from atomic wallet to this folder, the files need names in a specific format.");
                writer.WriteLine("The format is as follows: atomic-wallet-coin=awc=bnb1prf6nj3x0xk7lseg6ya3su0nqd77wyvn4v53g0.");
                writer.WriteLine("As you can see you write the name or id of the crypto currency and use an equals sign followed by the symbol for the crypto currency follow that by another equals sign with your address.");
            }
        }
    }

    private void GenerateFolders()
    {
        if (!Directory.Exists(TRANSACTIONS_FOLDER_PATH))
        {
            Directory.CreateDirectory(TRANSACTIONS_FOLDER_PATH);
        }

        for (int i = 0; i < SUPPORTED_TRANSACTIONS_FORMAT_PATHS.Length; i++)
        {
            if (!Directory.Exists(SUPPORTED_TRANSACTIONS_FORMAT_PATHS[i]))
            {
                Directory.CreateDirectory(SUPPORTED_TRANSACTIONS_FORMAT_PATHS[i]);
            }
        }
    }

    private void WriteOverview()
    {
        List<ITransactionModel> transactions = new List<ITransactionModel>();

        foreach (KeyValuePair<string, List<ITransactionModel>> currencyTransactionPair in _transactions)
        {
            transactions.AddRange(currencyTransactionPair.Value);
        }

        transactions.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));

        using (StreamWriter streamWriter = new StreamWriter(TRANSACTIONS_FOLDER_PATH + "/Overview.csv"))
        {
            streamWriter.WriteLine(CryptoDotComModel.CSV_DATA_ORDER);

            foreach (ITransactionModel transaction in transactions)
            {
                streamWriter.WriteLine(transaction.ToString());
            }
        }
    }

    public void RunCalculations()
    {
        HandleReversions();

        WriteOverview();

        List<ITransactionModel> purchaseTransactions = new List<ITransactionModel>();

        foreach (KeyValuePair<string, List<ITransactionModel>> currencyTransaction in _transactions)
        {
            foreach (ITransactionModel transaction in currencyTransaction.Value)
            {
                if (transaction.TransactionType == TransactionType.Purchase)
                {
                    purchaseTransactions.Add(transaction);
                }
                else if (transaction.TransactionType == TransactionType.Sale)
                {
                    decimal cryptoCurrencyAmountFromTransactions = 0;

                    for (int i = 0; i < purchaseTransactions.Count; i++)
                    {
                        cryptoCurrencyAmountFromTransactions += purchaseTransactions[i].CryptoCurrencyAmount;

                        if (cryptoCurrencyAmountFromTransactions >= transaction.CryptoCurrencyAmount)
                        {
                            List<ITransactionModel> purchaseTransactionsNeededToCoverSale = purchaseTransactions.Take(i + 1).ToList();

                            CalculateSaleFromPurchases(purchaseTransactionsNeededToCoverSale, transaction);

                            if (purchaseTransactions[i].CryptoCurrencyAmount > 0)
                            {
                                purchaseTransactions.RemoveRange(0, i);
                            }
                            else
                            {
                                purchaseTransactions.RemoveRange(0, i + 1);
                            }

                            break;
                        }
                    }
                }
            }

            purchaseTransactions.Clear();
        }

        _gains.Sort((x, y) => DateTime.Compare(x.PurchaseDate, y.PurchaseDate));
        _losses.Sort((x, y) => DateTime.Compare(x.PurchaseDate, y.PurchaseDate));

        if (_gains.Count > 0)
        {
            _gains[0].TotalAmount = _gains[0].Amount;

            for (int i = 1; i < _gains.Count; i++)
            {
                _gains[i].TotalAmount = _gains[i].Amount + _gains[i - 1].TotalAmount;
            }
        }

        if (_losses.Count > 0)
        {
            _losses[0].TotalAmount = _losses[0].Amount;

            for (int i = 1; i < _losses.Count; i++)
            {
                _losses[i].TotalAmount = _losses[i].Amount + _losses[i - 1].TotalAmount;
            }
        }

        WriteTansactionGains();
        WriteTranactionLosses();
    }

    private void HandleReversions()
    {
        foreach (KeyValuePair<string, List<ITransactionModel>> currencyTransactionPair in _transactions)
        {
            List<ITransactionModel> transactionsToRemove = new List<ITransactionModel>();
            List<ITransactionModel> reversionTransactions = currencyTransactionPair.Value.Where(t => t.TransactionType == TransactionType.Reversion).ToList();

            foreach (ITransactionModel reversionTransaction in reversionTransactions)
            {
                ITransactionModel revertedTransaction = currencyTransactionPair.Value.Where(t => t.CryptoCurrencyAmount == reversionTransaction.CryptoCurrencyAmount * -1 &&
                    t.TimeStamp < reversionTransaction.TimeStamp).OrderBy(t => Math.Abs((t.TimeStamp - reversionTransaction.TimeStamp).Ticks)).FirstOrDefault();

                if (revertedTransaction != null)
                {
                    reversionTransaction.CryptoCurrencyAmount = 0;
                    reversionTransaction.NativeAmount = 0;
                    revertedTransaction.CryptoCurrencyAmount = 0;
                    revertedTransaction.NativeAmount = 0;

                    transactionsToRemove.Add(reversionTransaction);
                    transactionsToRemove.Add(revertedTransaction);
                }
            }

            foreach (ITransactionModel transactionToRemove in transactionsToRemove)
            {
                currencyTransactionPair.Value.Remove(transactionToRemove);
            }
        }
    }

    private void CalculateSaleFromPurchases(List<ITransactionModel> purchaseTransactionsNeededToCoverSale, ITransactionModel saleTransaction)
    {
        foreach (ITransactionModel transaction in purchaseTransactionsNeededToCoverSale)
        {
            if (saleTransaction.CryptoCurrencyAmount - transaction.CryptoCurrencyAmount > 0)
            {
                saleTransaction.CryptoCurrencyAmount -= transaction.CryptoCurrencyAmount;
                transaction.CryptoCurrencyAmount = 0;
            }
            else
            {
                transaction.CryptoCurrencyAmount -= saleTransaction.CryptoCurrencyAmount;

            }

            decimal transactionProfit = (saleTransaction.ValueForOneCryptoTokenInNative - transaction.ValueForOneCryptoTokenInNative) * transaction.CryptoCurrencyAmount;

            if (transactionProfit < 0)
            {
                _losses.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, saleTransaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency));
            }
            else if (transactionProfit > 0)
            {
                _gains.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, saleTransaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency));
            }
        }
    }

    private void WriteTansactionGains()
    {
        using (StreamWriter streamWriter = new StreamWriter(TRANSACTIONS_FOLDER_PATH + "/Gains.csv"))
        {
            streamWriter.WriteLine(TaxableEvent.CSV_DATA_ORDER);

            foreach (TaxableEvent taxableEvent in _gains)
            {
                streamWriter.WriteLine(taxableEvent.ToString());
            }
        }
    }

    private void WriteTranactionLosses()
    {
        using (StreamWriter streamWriter = new StreamWriter(TRANSACTIONS_FOLDER_PATH + "/Losses.csv"))
        {
            streamWriter.WriteLine(TaxableEvent.CSV_DATA_ORDER);

            foreach (TaxableEvent taxableEvent in _losses)
            {
                streamWriter.WriteLine(taxableEvent.ToString());
            }
        }
    }

    private async Task InitializeTransaction(List<TransactionInformation> transactionInformations)
    {
        List<Task<List<ITransactionModel>>> transactions = new List<Task<List<ITransactionModel>>>();

        foreach (TransactionInformation transactionInformation in transactionInformations)
        {
            transactions.Add(_createTransactions[transactionInformation.TransactionType].Invoke(transactionInformation.FileName, transactionInformation.EntryData, ConvertTo));
        }

        List<ITransactionModel>[] transactionModels =  await Task.WhenAll(transactions);

        foreach (List<ITransactionModel> list in transactionModels)
        {
            foreach (ITransactionModel transaction in list)
            {
                if (!_transactions.ContainsKey(transaction.CryptoCurrency))
                {
                    _transactions.Add(transaction.CryptoCurrency, new List<ITransactionModel>());
                }

                _transactions[transaction.CryptoCurrency].Add(transaction);
            }
        }

        foreach (KeyValuePair<string, List<ITransactionModel>> pair in _transactions)
        {
            pair.Value.Sort((x, y) => x.CompareTo(y));
        }
    }

    private object ConvertTo(string value, Type fieldType)
    {
        if (fieldType == typeof(string))
        {
            return value;
        }

        if (fieldType == typeof(int))
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }
        }

        if (fieldType == typeof(decimal))
        {
            if (string.IsNullOrEmpty(value))
            {
                return decimal.Zero;
            }

            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal fResult))
            {
                return fResult;
            }
        }

        if (fieldType == typeof(DateTime))
        {
            DateTime result;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }
        }

        return null;
    }
}
