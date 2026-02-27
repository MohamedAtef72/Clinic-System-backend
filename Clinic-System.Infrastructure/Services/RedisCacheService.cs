using Clinic_System.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Clinic_System.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _mux;

        public RedisCacheService(IConnectionMultiplexer mux)
        {
            _mux = mux;
            _db = mux.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await _db.StringGetAsync(key);
            if (!val.HasValue) return default;
            return JsonSerializer.Deserialize<T>(val!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            var json = JsonSerializer.Serialize(value);
            if (absoluteExpiration.HasValue)
                await _db.StringSetAsync(key, json, absoluteExpiration.Value);
            else
                await _db.StringSetAsync(key, json);
        }

        // Simple versioning approach for cache invalidation
        public async Task<string> GetVersionAsync(string prefix)
        {
            var key = $"{prefix}:version";
            var v = await _db.StringGetAsync(key);
            if (v.HasValue) return v!;
            var initial = Guid.NewGuid().ToString();
            await _db.StringSetAsync(key, initial);
            return initial;
        }

        public async Task BumpVersionAsync(string prefix)
        {
            var key = $"{prefix}:version";
            await _db.StringSetAsync(key, Guid.NewGuid().ToString());
        }
    }
}