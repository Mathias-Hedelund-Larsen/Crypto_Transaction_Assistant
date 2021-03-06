﻿using System;

public interface ITransactionModel
{
    DateTime TimeStamp { get; set; }

    decimal NativeAmount { get; set; }

    string NativeCurrency { get; set; }

    string CryptoCurrency { get; set; }

    decimal CryptoCurrencyAmount { get; set; }

    decimal ValueForOneCryptoTokenInNative { get; set; }

    TransactionType TransactionType { get; set; }

    bool IsFullyTaxed { get; set; }

    string WalletName { get; }

    string TransactionId { get; }

    int CompareTo(ITransactionModel other);

    ITransactionModel Clone();
}
