using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TodoApi.Services
{
  public class CacheService : ICacheService
  {
    IDatabase _cacheDb;

    public CacheService()
    {
      string connection = Environment.GetEnvironmentVariable("RedisConnection", EnvironmentVariableTarget.Process);
      Console.WriteLine($"Connection String: {connection}");

      var configurationOptions = new ConfigurationOptions
      {
        EndPoints = { $"{connection}" },
      };
      var redis = ConnectionMultiplexer.Connect(configurationOptions);

      _cacheDb = redis.GetDatabase();
    }
    public T GetData<T>(string key)
    {
      var value = _cacheDb.StringGet(key);
      if (!String.IsNullOrWhiteSpace(value)) return JsonSerializer.Deserialize<T>(value);

      return default;
    }

    public object RemoveData(string key)
    {
      var _exist = _cacheDb.KeyExists(key);
      if (_exist) return _cacheDb.KeyDelete(key);

      return false;
    }

    public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
    {
      var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
      var isSet = _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);

      return isSet;
    }
  }
}