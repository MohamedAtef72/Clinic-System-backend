using Clinic_System.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Clinic_System.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _mux;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 100;

        public RedisCacheService(IConnectionMultiplexer mux)
        {
            _mux = mux ?? throw new ArgumentNullException(nameof(mux));
            _db = _mux.GetDatabase();
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
        {
            int attempt = 0;
            while (attempt < MaxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (RedisConnectionException ex) when (attempt < MaxRetries - 1)
                {
                    attempt++;
                    // Using Console.Error here previously; replace with logger via Console's output until DI logger is available.
                    // Log to Trace as a neutral sink — primary logging is available through DI in higher layers.
                    System.Diagnostics.Trace.TraceError($"Redis operation '{operationName}' failed (attempt {attempt}/{MaxRetries}): {ex.Message}. Retrying...");
                    await Task.Delay(RetryDelayMs * attempt);  // Exponential backoff
                }
                catch (TimeoutException ex) when (attempt < MaxRetries - 1)
                {
                    attempt++;
                    System.Diagnostics.Trace.TraceError($"Redis operation '{operationName}' timed out (attempt {attempt}/{MaxRetries}). Retrying...");
                    await Task.Delay(RetryDelayMs * attempt);  // Exponential backoff
                }
            }

            // Final attempt without catching
            return await operation();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return default;

                return await ExecuteWithRetryAsync(async () =>
                {
                    var val = await _db.StringGetAsync(key);
                    if (!val.HasValue)
                        return default;

                    return JsonSerializer.Deserialize<T>(val!);
                }, $"GetAsync({key})");
            }
            catch (RedisConnectionException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis connection error during GET: {ex.Message}");
                return default;  // Graceful fallback
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis timeout during GET: {ex.Message}");
                return default;  // Graceful fallback
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error getting from cache: {ex.Message}");
                return default;  // Graceful fallback
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

                var json = JsonSerializer.Serialize(value);

                await ExecuteWithRetryAsync(async () =>
                {
                    if (absoluteExpiration.HasValue)
                        await _db.StringSetAsync(key, json, absoluteExpiration.Value);
                    else
                        await _db.StringSetAsync(key, json);

                    return true;
                }, $"SetAsync({key})");
            }
            catch (RedisConnectionException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis connection error during SET: {ex.Message}");
                // Silently fail for cache sets - don't disrupt the application flow
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis timeout during SET: {ex.Message}");
                // Silently fail for cache sets - don't disrupt the application flow
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error setting cache: {ex.Message}");
                // Silently fail for cache sets - don't disrupt the application flow
            }
        }

        public async Task<string> GetVersionAsync(string prefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prefix))
                    throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));

                return await ExecuteWithRetryAsync(async () =>
                {
                    var key = $"{prefix}:version";
                    var v = await _db.StringGetAsync(key);
                    if (v.HasValue)
                        return v!.ToString();

                    var initial = Guid.NewGuid().ToString();
                    await _db.StringSetAsync(key, initial);
                    return initial;
                }, $"GetVersionAsync({prefix})");
            }
            catch (RedisConnectionException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis connection error during GetVersion: {ex.Message}");
                return Guid.NewGuid().ToString();  // Return new version on failure
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis timeout during GetVersion: {ex.Message}");
                return Guid.NewGuid().ToString();  // Return new version on failure
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error getting version: {ex.Message}");
                return Guid.NewGuid().ToString();  // Return new version on failure
            }
        }

        public async Task BumpVersionAsync(string prefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prefix))
                    throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));

                await ExecuteWithRetryAsync(async () =>
                {
                    var key = $"{prefix}:version";
                    await _db.StringSetAsync(key, Guid.NewGuid().ToString());
                    return true;
                }, $"BumpVersionAsync({prefix})");
            }
            catch (RedisConnectionException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis connection error during BumpVersion: {ex.Message}");
                // Silently fail - version bump is non-critical
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Trace.TraceError($"Redis timeout during BumpVersion: {ex.Message}");
                // Silently fail - version bump is non-critical
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error bumping version: {ex.Message}");
                // Silently fail - version bump is non-critical
            }
        }
    }
}
