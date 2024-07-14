using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Services.Interface
{
    public interface ICacheService
    {
        Task RemoveAsync(string key);
        Task<T> GetAsync<T>(string key);
        Task AddOrUpdateAsync<T>(string key, T value, TimeSpan? expiry = null);
    }
}
