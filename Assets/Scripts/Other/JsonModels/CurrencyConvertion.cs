using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public sealed class CurrencyConvertion 
{
    [SerializeField]
    private string _fromCurrency;

    [SerializeField]
    private string _toCurrency;

    [SerializeField]
    private JsonDateTime _dateOfConvertionRate;

    [SerializeField]
    private string _convertionRateS;

    private decimal _convertionRate = decimal.MinValue;

    public string FromToCurrencies => _fromCurrency + "->" + _toCurrency;

    public decimal ConvertionRate 
    {
        get 
        {
            if (_convertionRate == decimal.MinValue)
            {
                _convertionRate = decimal.Parse(_convertionRateS);
            }

            return _convertionRate;
        } 
    }

    public DateTime DateOfConvertionRate { get => _dateOfConvertionRate; }

    public CurrencyConvertion(string fromCurrency, string toCurrency, DateTime dateOfConvertionRate, decimal convertionRate)
    {
        _fromCurrency = fromCurrency;
        _toCurrency = toCurrency;
        _dateOfConvertionRate = dateOfConvertionRate;
        _convertionRate = convertionRate;
        _convertionRateS = convertionRate.ToString(CultureInfo.InvariantCulture);
    }
}
