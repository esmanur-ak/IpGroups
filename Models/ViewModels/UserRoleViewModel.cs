namespace IpGroups.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public bool IsMainAdmin { get; set; }
        public int DirectPermissionCount { get; set; }
        public List<RoleAssignItem> Roles { get; set; } = new();
    }

    public class RoleAssignItem
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
