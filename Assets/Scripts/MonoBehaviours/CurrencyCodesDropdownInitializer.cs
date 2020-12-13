using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public sealed class CurrencyCodesDropdownInitializer : MonoBehaviour
{
    private Dropdown _dropdown;
    private List<string> _currencyCodes;

    private async void Start()
    {
        _dropdown = GetComponent<Dropdown>();

        if (File.Exists(CryptoService.CURRENCY_CODES_FILE_PATH))
        {
            _currencyCodes = new List<string>(File.ReadAllText(CryptoService.CURRENCY_CODES_FILE_PATH).Split('\n'));

             MainComponent.Instance.TargetCurrency =_currencyCodes[0];

            _dropdown.options = _currencyCodes.Select(c => new Dropdown.OptionData(c)).ToList();
        }
        else
        {
           await GetApiCodes();
        }
    }

    private async Task GetApiCodes()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            string data = await httpClient.GetStringAsync($"https://openexchangerates.org/api/currencies.json");

            string[] dataSplit = data.Split('\n');

            _currencyCodes = new List<string>();

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            for (int i = 0; i < dataSplit.Length; i++)
            {
                if (dataSplit[i].Contains(":"))
                {
                    _currencyCodes.Add(dataSplit[i].Split(':')[0].Replace("\"", "").Trim());
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(CryptoService.CURRENCY_CODES_FILE_PATH))
            {
                streamWriter.Write(string.Join("\n", _currencyCodes));
            }

            MainComponent.Instance.TargetCurrency = _currencyCodes[0];

            _dropdown.options = _currencyCodes.Select(c => new Dropdown.OptionData(c)).ToList();
        }
    }

    public void SetTargetCurrency(int currency)
    {
        MainComponent.Instance.TargetCurrency = _currencyCodes[currency];
    }
}
