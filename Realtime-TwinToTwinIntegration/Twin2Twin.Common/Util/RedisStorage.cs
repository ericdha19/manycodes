using StackExchange.Redis;

using System;

namespace Twin2Twin.Common
{
    public static class RedisStorage
    {
        public static bool QueryWhiteList(string id)
        {
            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = Environment.GetEnvironmentVariable("whitelist").ToString();
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
            using (ConnectionMultiplexer redis = lazyConnection.Value)
            {
                IDatabase cache = redis.GetDatabase();
                var result = cache.StringGet(id);
                return !result.IsNull;
            }
        }
    }
}