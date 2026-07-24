using IpGroups.Data.DbContext;
using IpGroups.Models.Entities;
using IpGroups.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace IpGroups.Services.Concrete
{
    public class BaseService<T> : IService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseService(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<List<T>> GetAllAsync(bool trackChanges = false)
        {
            return trackChanges 
                ? await _dbSet.ToListAsync() 
                : await _dbSet.AsNoTracking().ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id, bool trackChanges = false)
        {
            if (trackChanges)
            {
                return await _dbSet.FindAsync(id);
            }
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public virtual async Task AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            
            // DbContext'te override edeceğimiz SaveChanges/SaveChangesAsync sayesinde
            // Remove çağrısı otomatik olarak soft-delete'e dönüştürülecektir.
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
