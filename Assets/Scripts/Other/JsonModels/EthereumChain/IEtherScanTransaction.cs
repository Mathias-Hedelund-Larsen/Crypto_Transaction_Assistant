using System;

namespace EthereumChain
{
    public interface IEtherScanTransaction
    {
        DateTime TimeStamp { get; }
        string Hash { get; }
        decimal Value { get; }
        string TokenName { get; }
        string TokenSymbol { get; }
    }
}
