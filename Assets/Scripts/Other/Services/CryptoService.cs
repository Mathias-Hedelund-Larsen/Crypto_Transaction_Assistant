using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

public sealed class CryptoService : ICryptoService
{
    public readonly static string TRANSACTIONS_FOLDER_PATH = Application.persistentDataPath + "/Crypto_Transactions";
    public readonly static string CURRENCY_CONVERTIONS = Application.persistentDataPath + "/CryptoApplicationData/CurrencyConvertions.Json";
    public readonly static string CURRENCY_CODES_FILE_PATH = Application.persistentDataPath + "/CryptoApplicationData/CurrencyCodes.txt";

    private readonly static string[] SUPPORTED_TRANSACTIONS_FORMAT_PATHS = new string[] 
    {
        TRANSACTIONS_FOLDER_PATH + "/CryptoDotCom",
        TRANSACTIONS_FOLDER_PATH + "/Celsius",
        TRANSACTIONS_FOLDER_PATH + "/AtomicWallet",
        TRANSACTIONS_FOLDER_PATH + "/CoinBase" 
    };

    private List<TaxableEvent> _gains = new List<TaxableEvent>();
    private List<TaxableEvent> _losses = new List<TaxableEvent>();
    private Dictionary<string, List<TransactionModelBase>> _transactions = new Dictionary<string, List<TransactionModelBase>>();

    public bool IsAnyTransactionAwaitingData
    {
        get
        {
            foreach (KeyValuePair<string, List<TransactionModelBase>> transaction in _transactions)
            {
                if (transaction.Value.Any(t => t.IsAwatingData))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public CryptoService()
    {
        GenerateFolders();

        GenerateReadMeInFolders();

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
                modelType = typeof(AtomicWalletModel);
            }

            foreach (string filePath in filePaths)
            {
                string[] lines = File.ReadAllLines(filePath);

                List<IEnumerable<TransactionModelBase>> transactions = lines.Skip(1).Select(line => InitializeTransaction(Path.GetFileName(filePath), line, modelType)).ToList();

                foreach (IEnumerable<TransactionModelBase> transaction in transactions)
                {
                    foreach (TransactionModelBase transactionModel in transaction)
                    {
                        if (!_transactions.ContainsKey(transactionModel.CryptoCurrency))
                        {
                            _transactions.Add(transactionModel.CryptoCurrency, new List<TransactionModelBase>());
                        }

                        _transactions[transactionModel.CryptoCurrency].Add(transactionModel);
                    }
                }
            }
        }

        foreach (KeyValuePair<string, List<TransactionModelBase>> pair in _transactions)
        {
            pair.Value.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));
        }
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
        List<TransactionModelBase> transactions = new List<TransactionModelBase>();

        foreach (KeyValuePair<string, List<TransactionModelBase>> currencyTransactionPair in _transactions)
        {
            transactions.AddRange(currencyTransactionPair.Value);
        }

        transactions.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));

        using (StreamWriter streamWriter = new StreamWriter(TRANSACTIONS_FOLDER_PATH + "/Overview.csv"))
        {
            streamWriter.WriteLine(TransactionModelBase.CSV_DATA_ORDER);

            foreach (TransactionModelBase transaction in transactions)
            {
                streamWriter.WriteLine(transaction.ToString());
            }
        }
    }

    public void RunCalculations()
    {
        HandleReversions();

        WriteOverview();

        List<TransactionModelBase> purchaseTransactions = new List<TransactionModelBase>();

        foreach (KeyValuePair<string, List<TransactionModelBase>> currencyTransaction in _transactions)
        {
            foreach (TransactionModelBase transaction in currencyTransaction.Value)
            {
                if(transaction.TransactionType == TransactionType.Purchase)
                {
                    purchaseTransactions.Add(transaction);
                }
                else if(transaction.TransactionType == TransactionType.Sale)
                {
                    decimal cryptoCurrencyAmountFromTransactions = 0;

                    for (int i = 0; i < purchaseTransactions.Count; i++)
                    {
                        cryptoCurrencyAmountFromTransactions += purchaseTransactions[i].CryptoCurrencyAmount;

                        if (cryptoCurrencyAmountFromTransactions >= transaction.CryptoCurrencyAmount)
                        {
                            List<TransactionModelBase> purchaseTransactionsNeededToCoverSale = purchaseTransactions.Take(i + 1).ToList();

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
    }

    private void HandleReversions()
    {
        foreach (KeyValuePair<string, List<TransactionModelBase>> currencyTransactionPair in _transactions)
        {
            List<TransactionModelBase> transactionsToRemove = new List<TransactionModelBase>();
            List<TransactionModelBase> reversionTransactions = currencyTransactionPair.Value.Where(t => t.TransactionType == TransactionType.Reversion).ToList();

            foreach (TransactionModelBase reversionTransaction in reversionTransactions)
            {
                TransactionModelBase revertedTransaction = currencyTransactionPair.Value.Where(t => t.CryptoCurrencyAmount == reversionTransaction.CryptoCurrencyAmount * -1 &&
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

            foreach (TransactionModelBase transactionToRemove in transactionsToRemove)
            {
                currencyTransactionPair.Value.Remove(transactionToRemove);
            }
        }
    }

    private void CalculateSaleFromPurchases(List<TransactionModelBase> purchaseTransactionsNeededToCoverSale, TransactionModelBase saleTransaction)
    {
        foreach (TransactionModelBase transaction in purchaseTransactionsNeededToCoverSale)
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
                if (_losses.Count > 0)
                {
                    _losses.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency,
                        _losses[_losses.Count - 1].TotalAmount + transactionProfit));
                }
                else
                {
                    _losses.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency,
                        transactionProfit));
                }
            }
            else if (transactionProfit > 0)
            {
                if (_gains.Count > 0)
                {
                    _gains.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency,
                        _gains[_gains.Count - 1].TotalAmount + transactionProfit));
                }
                else
                {
                    _gains.Add(new TaxableEvent(transaction.TransactionId, saleTransaction.TransactionId, transaction.TimeStamp, transactionProfit, MainComponent.Instance.TargetCurrency,
                        transactionProfit));
                }
            }
        }

        WriteTansactionGains();
        WriteTranactionLosses();
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

    private IEnumerable<TransactionModelBase> InitializeTransaction(string fileName, string entryData, Type transactionType)
    {
        TransactionModelBase data = (TransactionModelBase)FormatterServices.GetUninitializedObject(transactionType);

        return data.Init(fileName, entryData.Split(data.CSVSplit), ConvertTo);
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
