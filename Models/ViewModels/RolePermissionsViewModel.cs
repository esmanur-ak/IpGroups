namespace IpGroups.Models.ViewModels
{
    public class RolePermissionsViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<ModulePermissionGroup> ModuleGroups { get; set; } = new();
    }

    public class ModulePermissionGroup
    {
        public string ModuleName { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public string PermissionValue { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
