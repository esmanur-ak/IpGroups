using IpGroups.Models.Entities;

namespace IpGroups.Models.ViewModels
{
    public class IpAddressViewModel : BaseViewModel
    {
        public int IpGroupId { get; set; }
        public string? IpGroupName { get; set; }
        public string FullIpAddress { get; set; } = string.Empty;
        public int Octet { get; set; }
        public bool IsAssigned { get; set; }

        public static IpAddressViewModel FromEntity(IpAddress entity)
        {
            var viewModel = new IpAddressViewModel
            {
                IpGroupId = entity.IpGroupId,
                IpGroupName = entity.IpGroup?.IpNo,
                FullIpAddress = entity.FullIpAddress,
                Octet = entity.Octet,
                IsAssigned = entity.IsAssigned
            };
            viewModel.MapBaseFields(entity);
            return viewModel;
        }

        public IpAddress ToEntity()
        {
            var entity = new IpAddress
            {
                IpGroupId = this.IpGroupId,
                FullIpAddress = this.FullIpAddress,
                Octet = this.Octet,
                IsAssigned = this.IsAssigned
            };
            ToBaseEntity(entity);
            return entity;
        }
    }
}
