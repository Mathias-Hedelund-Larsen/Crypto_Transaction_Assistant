using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class CurrencyConvertionsContainer 
{
    [SerializeField]
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
