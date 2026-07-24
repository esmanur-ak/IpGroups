namespace IpGroups.Models.Entities
{
    public class IpAddress : BaseEntity
    {
        public int IpGroupId { get; set; }
        public Ip IpGroup { get; set; } = null!;
        public string FullIpAddress { get; set; } = string.Empty;
        public int Octet { get; set; }
        public bool IsAssigned { get; set; }
    }
}
