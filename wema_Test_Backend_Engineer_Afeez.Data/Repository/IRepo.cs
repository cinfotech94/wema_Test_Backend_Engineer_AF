using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Data.Repository
{
    public interface IRepo
    {
        Task<T?> GetAsync<T>(int id) where T : class;
        Task<IQueryable<T>> GetAsync<T>() where T : class;
        Task<int> AddAsync<T>(T entity) where T : class;
    }
}
