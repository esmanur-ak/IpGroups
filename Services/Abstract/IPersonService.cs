using IpGroups.Models.Entities;

namespace IpGroups.Services.Abstract
{
    public interface IPersonService : IService<Person>
    {
        Task<List<Person>> GetAllWithDetailsAsync(); // İlişkileriyle birlikte tüm personelleri çeker
        Task<Person?> GetByIdWithDetailsAsync(int id); // Detaylı tek bir personel getirir
    }
}
