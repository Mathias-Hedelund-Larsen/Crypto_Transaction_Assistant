using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed class AtomicWalletModel : BlockChainTransactionModelBase
{
    private const string SALE = "sale";

    private const string PURCHASE = "purchase";

    private static readonly string[] NON_TAXABLE_EVENTS = new string[]
    {
        "Incoming",
        "Outgoing"
    };

    public override string WalletName => "Atomic wallet";

    public override char CSVSplit => ';';

    public override IEnumerable<TransactionModelBase> Init(string fileName, string[] entryData, Func<string, Type, object> convertFromString)
    {
        string[] coinIdSymbolAndAddress = fileName.Split('=');

        if (coinIdSymbolAndAddress.Length < 3)
        {
            return Enumerable.Empty<TransactionModelBase>();
        }
        else
        {
            return InternalInit(coinIdSymbolAndAddress[0].ToLower(), coinIdSymbolAndAddress[1].ToUpper(), coinIdSymbolAndAddress[2], entryData, convertFromString);
        }
    }

    private IEnumerable<TransactionModelBase> InternalInit(string coinId, string coinSymbol, string address, string[] entryData, Func<string, Type, object> convertFromString)
    {
        _address = address;
        _transactionId = entryData[0].RemoveQuotationMarks();

        CryptoCurrency = coinSymbol;
        CryptoCurrencyAmount = (decimal)convertFromString.Invoke(entryData[4].RemoveQuotationMarks().RemoveWhiteSpaces(), typeof(decimal));

        TimeStamp = (DateTime)convertFromString.Invoke(entryData[2].RemoveQuotationMarks().Remove(24), typeof(DateTime));

        string transactionKind = entryData[3].ToLower().RemoveQuotationMarks();

        if (transactionKind == SALE)
        {
            TransactionType = TransactionType.Sale;
        }
        else if (transactionKind == PURCHASE)
        {
            TransactionType = TransactionType.Purchase;
        }

        if (entryData[1].Contains(BINANCE_INDICATOR))
        {
            _blockChainToCall = BlockChain.Binance;
        }

        MainComponent.Instance.StartCoroutine(Init(coinId));

        yield return this;
    }

    public override TransactionModelBase Clone()
    {
        return (AtomicWalletModel)MemberwiseClone();
    }
}
