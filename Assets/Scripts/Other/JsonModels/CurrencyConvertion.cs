using Newtonsoft.Json;
using System;

[Serializable]
public sealed class CurrencyConvertion 
{
    [JsonProperty]
    private string _fromCurrency;

    [JsonProperty]
    private string _toCurrency;

    [JsonProperty]
    private decimal _convertionRate;

    [JsonProperty]
    private DateTime _dateOfConvertionRate;

    [JsonIgnore]
    public string FromToCurrencies => _fromCurrency + "->" + _toCurrency;

    [JsonIgnore]
    public decimal ConvertionRate  => _convertionRate;

    [JsonIgnore]
    public DateTime DateOfConvertionRate => _dateOfConvertionRate;

    public CurrencyConvertion(string fromCurrency, string toCurrency, DateTime dateOfConvertionRate, decimal convertionRate)
    {
        _fromCurrency = fromCurrency;
        _toCurrency = toCurrency;
        _dateOfConvertionRate = dateOfConvertionRate;
        _convertionRate = convertionRate;
    }
}
