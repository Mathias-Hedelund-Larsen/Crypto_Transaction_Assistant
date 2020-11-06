using System.Diagnostics;
using System.IO;
using UnityEngine;

public class MainComponent : MonoBehaviour
{
    [SerializeField]
    private string _targetCurrency = "DKK";

    private CurrencyConvertionsContainer _currencyConvertionsContainer;

    private ICryptoService _cryptoService;

    public string TargetCurrency { get { return _targetCurrency; } set { _targetCurrency = value; } }

    public static MainComponent Instance { get; private set; }

    public CurrencyConvertionsContainer CurrencyConvertionsContainer => _currencyConvertionsContainer;

    private void Awake()
    {
        Instance = this;
        enabled = false;

        if(!Directory.Exists(Application.persistentDataPath + "/CryptoApplicationData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/CryptoApplicationData");
        }

        if (File.Exists(CryptoService.CURRENCY_CONVERTIONS))
        {
            _currencyConvertionsContainer = JsonUtility.FromJson<CurrencyConvertionsContainer>(File.ReadAllText(CryptoService.CURRENCY_CONVERTIONS));
        }
        else
        {
            _currencyConvertionsContainer = new CurrencyConvertionsContainer();
        }

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

    private void OnDestroy()
    {
        using (StreamWriter streamWriter = new StreamWriter(CryptoService.CURRENCY_CONVERTIONS))
        {
            streamWriter.Write(JsonUtility.ToJson(_currencyConvertionsContainer));
        }
    }
}
