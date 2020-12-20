using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class AtomicWalletModel : BlockChainTransactionModel
{
    private const string SALE = "sale";

    private const string PURCHASE = "purchase";

    private static List<string> _ethAddressesAdded = new List<string>();

    private static readonly string[] NON_TAXABLE_EVENTS = new string[]
    {
        "Incoming",
        "Outgoing"
    };

    public override string WalletName => "Atomic wallet";

    private AtomicWalletModel()
    {

    }

    public static async Task<List<ITransactionModel>> InitializeFromData(string fileName, string entryData, Func<string, Type, object> convertFromString)
    {
        string[] coinIdSymbolAndAddress = fileName.Split('=');

        if (coinIdSymbolAndAddress.Length != 3)
        {
            return new List<ITransactionModel>();
        }
        else
        {
            return await InternalInit(coinIdSymbolAndAddress[0].ToLower(), coinIdSymbolAndAddress[1].ToUpper(), coinIdSymbolAndAddress[2].Split('.')[0], entryData.Split(';'), convertFromString);
        }
    }

    private static async Task<List<ITransactionModel>> InternalInit(string coinId, string coinSymbol, string address, string[] entryData, Func<string, Type, object> convertFromString)
    {
        AtomicWalletModel atomicWalletModel = new AtomicWalletModel();
        atomicWalletModel._address = address;
        atomicWalletModel._transactionId = entryData[0].RemoveQuotationMarks();

        atomicWalletModel.CryptoCurrency = coinSymbol;
        atomicWalletModel.TimeStamp = (DateTime)convertFromString.Invoke(entryData[2].RemoveQuotationMarks().Remove(24), typeof(DateTime));

        string transactionKind = entryData[3].ToLower().RemoveQuotationMarks();

        if (transactionKind == SALE)
        {
            atomicWalletModel.TransactionType = TransactionType.Sale;
        }
        else if (transactionKind == PURCHASE)
        {
            atomicWalletModel.TransactionType = TransactionType.Purchase;
        }

        if (entryData[1].Contains(BINANCE_INDICATOR))
        {
            atomicWalletModel._blockChainToCall = BlockChain.Binance;
        }
        else if (entryData[1].Contains(ETHEREUM_INDICATOR))
        {
            atomicWalletModel._blockChainToCall = BlockChain.Ethereum;

            if (_ethAddressesAdded.Contains(address))
            {
                return new List<ITransactionModel>();
            }

            _ethAddressesAdded.Add(address);
        }

        return await atomicWalletModel.Init(coinId);
    }
}
