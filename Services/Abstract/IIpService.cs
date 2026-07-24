using IpGroups.Models.Entities;

namespace IpGroups.Services.Abstract
{
    public interface IIpService : IService<Ip>
    {
        Task<List<Ip>> GetUnassignedIpsAsync(); // Personellere atanmamış boşta duran IP'leri listelemek için özel metot
        Task<List<Ip>> GetAllWithAddressesAsync(); // IP adresleriyle birlikte (Include ile) getirme metodu
    }
}