namespace IpGroups.Models.Entities
{
    public class Ip:BaseEntity
    {
        public string IpNo { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<IpAddress> IpAddresses { get; set; } = new List<IpAddress>();
    }
}
