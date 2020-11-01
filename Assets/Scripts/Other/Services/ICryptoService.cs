public interface ICryptoService
{
    bool IsAnyTransactionAwaitingData { get; }

    void RunCalculations();
}
