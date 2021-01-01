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
    private TransactionTracking _transactionTracking;
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

        if (File.Exists(CryptoService.CURRENCY_CONVERTIONS_PATH))
        {
            _currencyConvertionsContainer = JsonConvert.DeserializeObject<CurrencyConvertionsContainer>(File.ReadAllText(CryptoService.CURRENCY_CONVERTIONS_PATH));
        }
        else
        {
            _currencyConvertionsContainer = new CurrencyConvertionsContainer();
        }

        var list = new System.Collections.Generic.List<AddressInfo>();
        list.Add(new AddressInfo("test", BlockChain.Binance, new System.Collections.Generic.List<TransactionIdAndType> { new TransactionIdAndType("tx", TransactionType.Purchase) }));
        var data = JsonConvert.SerializeObject(new TransactionTracking(list));

        UnityEngine.Debug.Log(data);

        _cryptoService = new CryptoService();
    }

    public void AddTransactionTracking(TransactionTracking transactionTracking)
    {
        _transactionTracking = transactionTracking;
    }

    public void OpenTransactionsFolder()
    {
        Process.Start(CryptoService.TRANSACTIONS_FOLDER_PATH);
    }

    public void Execute()
    {
        Task.Run(() => UpdateCurrencies.Invoke()).Wait();
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
        using (StreamWriter streamWriter = new StreamWriter(CryptoService.CURRENCY_CONVERTIONS_PATH))
        {
            streamWriter.Write(JsonConvert.SerializeObject(_currencyConvertionsContainer));
        }

        using (StreamWriter streamWriter = new StreamWriter(CryptoService.TRANSACTIONS_TRACKING_PATH))
        {
            streamWriter.Write(JsonConvert.SerializeObject(_transactionTracking));
        }
    }
}
