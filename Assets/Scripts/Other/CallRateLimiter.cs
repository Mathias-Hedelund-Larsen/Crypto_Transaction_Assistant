using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public sealed class CallRateLimiter : IDisposable
{
    private static Dictionary<string, SemaphoreSlim> _lockKeys = new Dictionary<string, SemaphoreSlim>();

    private readonly string _lockKey;
    private readonly TimeSpan _interval;
    private readonly HttpClient _httpClient;

    public CallRateLimiter(string lockKey, int callsAllowedEachInterval, TimeSpan interval)
    {
        _lockKey = lockKey;
        _interval = interval;

        _httpClient = new HttpClient();

        lock (_lockKeys) 
        { 
            if (!_lockKeys.ContainsKey(lockKey))
            {
                _lockKeys.Add(lockKey, new SemaphoreSlim(callsAllowedEachInterval, callsAllowedEachInterval));
            }
        }
    }

    public async Task<HttpResponseMessage> GetDataAsync(string query)
    {
        while(!await _lockKeys[_lockKey].WaitAsync(TimeSpan.FromMilliseconds(1)))
        {
            await Task.Delay(_interval);
        }
        
        HttpResponseMessage data = await _httpClient.GetAsync(query);

        _lockKeys[_lockKey].Release(1);

        return data;
    }

    public void Dispose()
    {
        _httpClient.Dispose();       
    }
}
