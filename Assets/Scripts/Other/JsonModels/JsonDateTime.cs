using System;
using UnityEngine;

[Serializable]
public struct JsonDateTime 
{
    private static readonly DateTime EPOCH_START = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [SerializeField]
    private long _dateEpochSeconds;

    public static implicit operator DateTime(JsonDateTime jdt)
    {
        return EPOCH_START.AddSeconds(jdt._dateEpochSeconds);
    }
    public static implicit operator JsonDateTime(DateTime dt)
    {
        JsonDateTime jdt = new JsonDateTime();
        jdt._dateEpochSeconds = (long)(dt - EPOCH_START).TotalSeconds; 

        return jdt;
    }
}
