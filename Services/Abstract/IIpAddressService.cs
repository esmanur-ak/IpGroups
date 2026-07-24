using IpGroups.Models.Entities;

namespace IpGroups.Services.Abstract
{
    public interface IIpAddressService : IService<IpAddress>
    {
        Task<List<IpAddress>> GetByIpGroupIdAsync(int ipGroupId);
        Task<List<IpAddress>> GetUnassignedByGroupIdAsync(int ipGroupId);
        Task SyncAllIpAddressesAsync();
    }
}
