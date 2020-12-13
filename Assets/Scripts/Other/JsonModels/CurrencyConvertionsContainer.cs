using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class CurrencyConvertionsContainer 
{
    [JsonProperty]
    private List<CurrencyConvertion> _currencyConvertions = new List<CurrencyConvertion>();

    [NonSerialized]
    private Dictionary<string, List<CurrencyConvertion>> _fromToCurrencies;

    public CurrencyConvertion this[string key, DateTime date]
    {
        get
        {
            Init();

            if (_fromToCurrencies.ContainsKey(key))
            {
                return _fromToCurrencies[key].Find(c => c.DateOfConvertionRate == date);
            }

            return null;
        }
    }

    private void Init()
    {
        if (_fromToCurrencies == null)
        {
            _fromToCurrencies = _currencyConvertions.GroupBy(c => c.FromToCurrencies).ToDictionary(c => c.Key, c => c.ToList());
        }
    }

    public bool ContainsKey(string key, DateTime date)
    {
        Init();

        if (_fromToCurrencies.ContainsKey(key))
        {
            return _fromToCurrencies[key].Any(c => c.DateOfConvertionRate == date);
        }

        return false;
    }

    public void Add(CurrencyConvertion currencyConvertion)
    {
        Init();

        if (_fromToCurrencies.ContainsKey(currencyConvertion.FromToCurrencies))
        {
            if (!_fromToCurrencies[currencyConvertion.FromToCurrencies].Any(c => c.DateOfConvertionRate == currencyConvertion.DateOfConvertionRate))
            {
                _fromToCurrencies[currencyConvertion.FromToCurrencies].Add(currencyConvertion);

                _currencyConvertions.Add(currencyConvertion);
            }
        }
        else
        {
            _fromToCurrencies.Add(currencyConvertion.FromToCurrencies, new List<CurrencyConvertion> { currencyConvertion });
            _currencyConvertions.Add(currencyConvertion);
        }
    }
}
