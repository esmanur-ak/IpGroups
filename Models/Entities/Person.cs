namespace IpGroups.Models.Entities
{
    public class Person : BaseEntity
    {
        
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public int? BinaId { get; set; }
        public Bina? Bina { get; set; }

        public int? BirimId { get; set; }
        public Birim? Birim { get; set; }

        public int? IpAddressId { get; set; }
        public IpAddress? IpAddress { get; set; }
        public string Domain { get; set; } = string.Empty;
    }
}
