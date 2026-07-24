using IpGroups.Models.Entities;

namespace IpGroups.Models.ViewModels
{
    public class BaseViewModel
    {
        // Türkiye saat dilimi UTC+3 - sabit offset ile gösterim
        private static readonly TimeSpan _turkeyOffset = TimeSpan.FromHours(3);

        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
        public DateTime? DeletedDate { get; set; }

        // UTC tarihi Türkiye yerel saatine (+3) çevirir
        private static DateTime ToTurkeyTime(DateTime utcDate)
        {
            if (utcDate == default || utcDate <= new DateTime(2000, 1, 1))
                return utcDate;
            if (utcDate.Kind == DateTimeKind.Unspecified)
                utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            return utcDate + _turkeyOffset;
        }

        private static DateTime? ToTurkeyTime(DateTime? utcDate)
            => utcDate.HasValue ? ToTurkeyTime(utcDate.Value) : null;

        protected void MapBaseFields(BaseEntity entity)
        {
            Id = entity.Id;
            IsDeleted = entity.IsDeleted;
            CreatedDate = ToTurkeyTime(entity.CreatedDate);
            UpdatedDate = ToTurkeyTime(entity.UpdatedDate);
            CreatedBy = entity.CreatedBy;
            UpdatedBy = entity.UpdatedBy;
            DeletedBy = entity.DeletedBy;
            DeletedDate = ToTurkeyTime(entity.DeletedDate);
        }

        protected void ToBaseEntity(BaseEntity entity)
        {
            entity.Id = Id;
            entity.IsDeleted = IsDeleted;
            entity.CreatedDate = CreatedDate;
            entity.UpdatedDate = UpdatedDate;
            entity.CreatedBy = CreatedBy;
            entity.UpdatedBy = UpdatedBy;
            entity.DeletedBy = DeletedBy;
            entity.DeletedDate = DeletedDate;
        }
    }
}