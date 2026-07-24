namespace IpGroups.Models.ViewModels
{
    public class UserPermissionsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<ModulePermissionGroup> ModuleGroups { get; set; } = new();
    }
}
