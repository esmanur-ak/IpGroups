using IpGroups.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace IpGroups.Models.ViewModels
{
    public class BinaViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Bina adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Bina adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        public static BinaViewModel FromEntity(Bina entity)
        {
            var viewModel = new BinaViewModel
            {
                Name = entity.Name
            };
            viewModel.MapBaseFields(entity);
            return viewModel;
        }

        public Bina ToEntity()
        {
            var entity = new Bina
            {
                Name = this.Name
            };
            ToBaseEntity(entity);
            return entity;
        }
    }
}