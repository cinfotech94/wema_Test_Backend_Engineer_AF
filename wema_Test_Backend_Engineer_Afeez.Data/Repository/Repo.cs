using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wema_Test_Backend_Engineer_Afeez.Data.Repository
{
    public class Repo : IRepo
    {
        private readonly AppDbContext _context;

        public Repo(AppDbContext dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public async Task<int> AddAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
        public async Task<IQueryable<T>> GetAsync<T>() where T : class
        {
            return _context.Set<T>().AsQueryable();
        }

        public async Task<T?> GetAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }
    }
}
