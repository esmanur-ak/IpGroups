using IpGroups.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace IpGroups.Models.ViewModels
{
    public class PersonViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
        public string Soyad { get; set; } = string.Empty;

        public int? BinaId { get; set; }
        public BinaViewModel? Bina { get; set; }

        public int? BirimId { get; set; }
        public BirimViewModel? Birim { get; set; }

        public int? IpAddressId { get; set; }
        public IpAddressViewModel? IpAddress { get; set; }

        [Required(ErrorMessage = "Domain adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Domain adı en fazla 200 karakter olabilir.")]
        public string Domain { get; set; } = string.Empty;

        public static PersonViewModel FromEntity(Person entity)
        {
            var viewModel = new PersonViewModel
            {
                Ad = entity.Ad,
                Soyad = entity.Soyad,
                BinaId = entity.BinaId,
                Bina = entity.Bina != null ? BinaViewModel.FromEntity(entity.Bina) : null,
                BirimId = entity.BirimId,
                Birim = entity.Birim != null ? BirimViewModel.FromEntity(entity.Birim) : null,
                IpAddressId = entity.IpAddressId,
                IpAddress = entity.IpAddress != null ? IpAddressViewModel.FromEntity(entity.IpAddress) : null,
                Domain = entity.Domain
            };
            viewModel.MapBaseFields(entity);
            return viewModel;
        }

        public Person ToEntity()
        {
            var entity = new Person
            {
                Ad = this.Ad,
                Soyad = this.Soyad,
                BinaId = this.BinaId,
                BirimId = this.BirimId,
                IpAddressId = this.IpAddressId,
                Domain = this.Domain
            };
            ToBaseEntity(entity);
            return entity;
        }
    }
}