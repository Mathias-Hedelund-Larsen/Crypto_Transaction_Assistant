using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public sealed class CurrencyCodesDropdownInitializer : MonoBehaviour
{
    private Dropdown _dropdown;
    private List<string> _currencyCodes;

    private void Start()
    {
        _dropdown = GetComponent<Dropdown>();

        StartCoroutine(GetApiCodes());
    }

    private IEnumerator GetApiCodes()
    {
        using (UnityWebRequest currencyCodesRequest = UnityWebRequest.Get($"https://openexchangerates.org/api/currencies.json"))
        {
            yield return currencyCodesRequest.SendWebRequest();

            if (currencyCodesRequest.isNetworkError)
            {
            }
            else
            {
                string data = currencyCodesRequest.downloadHandler.text;

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

                MainComponent.Instance.TargetCurrency = _currencyCodes[0];

                _dropdown.AddOptions(_currencyCodes.Select(c => new Dropdown.OptionData(c)).ToList());
            }
        }
    }

    public void SetTargetCurrency(int currencyIndex)
    {
        MainComponent.Instance.TargetCurrency = _currencyCodes[currencyIndex];
    }
}
