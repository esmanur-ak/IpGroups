using IpGroups.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace IpGroups.Models.ViewModels
{
    public class IpViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "IP adresi zorunludur.")]
        [RegularExpression(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Geçersiz IP adresi formatı.")]
        public string IpNo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        public static IpViewModel FromEntity(Ip entity)
        {
            var viewModel = new IpViewModel
            {
                IpNo = entity.IpNo,
                Description = entity.Description
            };
            viewModel.MapBaseFields(entity);
            return viewModel;
        }

        public Ip ToEntity()
        {
            var entity = new Ip
            {
                IpNo = this.IpNo,
                Description = this.Description
            };
            ToBaseEntity(entity);
            return entity;
        }
    }
}