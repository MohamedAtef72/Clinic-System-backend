using System;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null);
        Task<string> GetVersionAsync(string prefix);
        Task BumpVersionAsync(string prefix);
    }
}