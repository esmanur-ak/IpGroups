using IpGroups.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace IpGroups.Models.ViewModels
{
    public class BirimViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Birim adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Birim adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        public static BirimViewModel FromEntity(Birim entity)
        {
            var viewModel = new BirimViewModel
            {
                Name = entity.Name
            };
            viewModel.MapBaseFields(entity);
            return viewModel;
        }

        public Birim ToEntity()
        {
            var entity = new Birim
            {
                Name = this.Name
            };
            ToBaseEntity(entity);
            return entity;
        }
    }
}