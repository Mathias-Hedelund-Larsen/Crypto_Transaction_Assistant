using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainComponent : MonoBehaviour
{
    [SerializeField]
    private string _targetCurrency = "DKK";

    [SerializeField]
    private Dropdown _culture;

    [SerializeField]
    private bool _isExecuting;

    private CurrencyConvertionsContainer _currencyConvertionsContainer;

    private ICryptoService _cryptoService;
    private CultureInfo[] _cultures;
    private TransactionTracking _transactionTracking;
    public Func<Task> UpdateCurrencies { get; set; }

    public static MainComponent Instance { get; private set; }

    public CurrencyConvertionsContainer CurrencyConvertionsContainer => _currencyConvertionsContainer;

    public string TargetCurrency { get => _targetCurrency; set => _targetCurrency = value; }
    public CultureInfo CurrentCulture { get; private set; }

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

        _cryptoService = new CryptoService();

        _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        Array.Sort(_cultures, (x, y) => x.Name.CompareTo(y.Name));

        _culture.AddOptions(_cultures.Select(c => new Dropdown.OptionData(c.Name)).ToList());
        CurrentCulture = CultureInfo.CurrentCulture;

        _culture.value = Array.IndexOf(_cultures, CurrentCulture);
    }

    public void UpdateCulture(int index)
    {
        CurrentCulture = _cultures[index];
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
        if (!_isExecuting)
        {
            _isExecuting = true;

            Task.Run(() => UpdateCurrencies.Invoke()).ContinueWith(RunCalculations);
        }
    }

    private async void RunCalculations(Task currencyTask)
    {
        await currencyTask;

        await Task.Run(_cryptoService.RunCalculations);

        _isExecuting = false;
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
