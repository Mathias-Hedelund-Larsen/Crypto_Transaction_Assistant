using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class MainComponent : MonoBehaviour
{
    [SerializeField]
    private string _targetCurrency = "DKK";

    private CurrencyConvertionsContainer _currencyConvertionsContainer;

    private ICryptoService _cryptoService;
    private bool _runCalculations = false;

    public Func<Task> UpdateCurrencies { get; set; }

    public static MainComponent Instance { get; private set; }

    public CurrencyConvertionsContainer CurrencyConvertionsContainer => _currencyConvertionsContainer;

    public string TargetCurrency { get => _targetCurrency; set => _targetCurrency = value; }

    private void Awake()
    {
        Instance = this;

        if(!Directory.Exists(Application.persistentDataPath + "/CryptoApplicationData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/CryptoApplicationData");
        }

        if (File.Exists(CryptoService.CURRENCY_CONVERTIONS))
        {
            _currencyConvertionsContainer = JsonConvert.DeserializeObject<CurrencyConvertionsContainer>(File.ReadAllText(CryptoService.CURRENCY_CONVERTIONS));
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

    public async void Execute()
    {
        await UpdateCurrencies.Invoke();
        _runCalculations = true;
    }

    private void Update()
    {
        if (_runCalculations)
        {
            _cryptoService.RunCalculations();

            _runCalculations = false;
        }
    }

    private void OnDestroy()
    {
        using (StreamWriter streamWriter = new StreamWriter(CryptoService.CURRENCY_CONVERTIONS))
        {
            streamWriter.Write(JsonConvert.SerializeObject(_currencyConvertionsContainer));
        }
    }
}
