using System.Diagnostics;
using UnityEngine;

public class MainComponent : MonoBehaviour
{
    [SerializeField]
    private string _targetCurrency = "DKK";

    private ICryptoService _cryptoService;

    public string TargetCurrency { get { return _targetCurrency; } set { _targetCurrency = value; } }

    public static MainComponent Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        enabled = false;
        _cryptoService = new CryptoService();
    }

    public void OpenTransactionsFolder()
    {
        Process.Start(CryptoService.TRANSACTIONS_FOLDER_PATH);
    }

    public void Execute()
    {
        enabled = true;
    }

    private void Update()
    {
        if (!_cryptoService.IsAnyTransactionAwaitingData)
        {
            _cryptoService.RunCalculations();

            enabled = false;
        }
    }
}
