using IpGroups.Models.Entities;

namespace IpGroups.Services.Abstract
{
    public interface IService<T> where T : BaseEntity
    {
        Task<List<T>> GetAllAsync(bool trackChanges = false);
        Task<T?> GetByIdAsync(int id, bool trackChanges = false);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}
