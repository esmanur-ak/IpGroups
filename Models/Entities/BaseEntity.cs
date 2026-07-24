namespace IpGroups.Models.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
        public DateTime? DeletedDate { get; set; }
    }
}
