using IpGroups.Constants;
using IpGroups.Models.Entities;
using IpGroups.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IpGroups.Controllers
{
    [Authorize(Policy = Permissions.Role.Manage)]
    public class RoleController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleViewModels = new List<RoleViewModel>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleViewModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Description = role.Description,
                    UserCount = usersInRole.Count
                });
            }

            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);
                var directPermCount = userClaims.Count(c => c.Type == "Permission");
                bool isMainAdmin = string.Equals(user.Email, "admin@ipgroups.com", StringComparison.OrdinalIgnoreCase);

                userList.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    AdSoyad = user.AdSoyad,
                    IsMainAdmin = isMainAdmin,
                    DirectPermissionCount = directPermCount,
                    Roles = userRoles.Select(r => new RoleAssignItem { RoleName = r, IsAssigned = true }).ToList()
                });
            }

            ViewBag.Users = userList;
            ViewBag.AllModuleGroups = BuildModuleGroups(new HashSet<string>());
            return View(roleViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors });
            }

            if (await _roleManager.RoleExistsAsync(model.Name))
            {
                return Json(new { success = false, errors = new[] { "Bu isimde bir rol zaten mevcut." } });
            }

            var role = new AppRole
            {
                Name = model.Name,
                Description = model.Description
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Bu e-posta adresi ile kayıtlı başka bir kullanıcı mevcut." });
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                AdSoyad = model.AdSoyad,
                Gender = model.Gender,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }

            // Yeni eklenen kullanıcı standart "User" rolü alır
            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            if (model.SelectedPermissions != null && model.SelectedPermissions.Any())
            {
                foreach (var perm in model.SelectedPermissions.Distinct())
                {
                    await _userManager.AddClaimAsync(user, new Claim("Permission", perm));
                }
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email!,
                AdSoyad = user.AdSoyad,
                IsMainAdmin = string.Equals(user.Email, "admin@ipgroups.com", StringComparison.OrdinalIgnoreCase),
                Roles = roles.Select(role => new RoleAssignItem
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    IsAssigned = userRoles.Contains(role.Name!)
                }).ToList()
            };

            return Json(new { success = true, data = model });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            bool isMainAdmin = string.Equals(user.Email, "admin@ipgroups.com", StringComparison.OrdinalIgnoreCase);
            var currentRoles = await _userManager.GetRolesAsync(user);

            foreach (var item in model.Roles)
            {
                if (isMainAdmin && item.RoleName == "Ana Yönetici" && !item.IsAssigned)
                {
                    continue; // Ana Yönetici Ana Yönetici rolünden çıkarılamaz
                }

                if (item.IsAssigned && !currentRoles.Contains(item.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else if (!item.IsAssigned && currentRoles.Contains(item.RoleName))
                {
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var activePermissionValues = userClaims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToHashSet();

            var vm = new UserPermissionsViewModel
            {
                UserId = user.Id,
                AdSoyad = user.AdSoyad,
                Email = user.Email!,
                ModuleGroups = BuildModuleGroups(activePermissionValues)
            };

            return Json(new { success = true, data = vm });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserPermissions([FromBody] UserPermissionsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            var currentClaims = await _userManager.GetClaimsAsync(user);
            var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

            foreach (var claim in permissionClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            foreach (var group in model.ModuleGroups)
            {
                foreach (var perm in group.Permissions)
                {
                    if (perm.IsSelected)
                    {
                        await _userManager.AddClaimAsync(user, new Claim("Permission", perm.PermissionValue));
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return Json(new { success = false, message = "Rol bulunamadı." });
            }

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var activePermissionValues = roleClaims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToHashSet();

            var vm = new RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name!,
                ModuleGroups = BuildModuleGroups(activePermissionValues)
            };

            return Json(new { success = true, data = vm });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] RolePermissionsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                return Json(new { success = false, message = "Rol bulunamadı." });
            }

            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = currentClaims.Where(c => c.Type == "Permission").ToList();

            foreach (var claim in permissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var group in model.ModuleGroups)
            {
                foreach (var perm in group.Permissions)
                {
                    if (perm.IsSelected)
                    {
                        await _roleManager.AddClaimAsync(role, new Claim("Permission", perm.PermissionValue));
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Rol bulunamadı." });
            }

            if (role.Name == "Ana Yönetici" || role.Name == "User")
            {
                return Json(new { success = false, message = "Sistem varsayılan 'Ana Yönetici' veya 'User' rolleri silinemez." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Rol silinirken bir hata oluştu." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            if (string.Equals(user.Email, "admin@ipgroups.com", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Ana sistem yöneticisi (Sistem Yöneticisi) hesabı silinemez." });
            }

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return Json(new { success = false, message = "Kendi hesabınızı silemezsiniz." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Kullanıcı silinirken bir hata oluştu." });
        }

        private static List<ModulePermissionGroup> BuildModuleGroups(HashSet<string> activePermissionValues)
        {
            return new List<ModulePermissionGroup>
            {
                new ModulePermissionGroup
                {
                    ModuleName = "Bina Yönetimi",
                    Permissions = new List<PermissionItem>
                    {
                        new() { PermissionValue = Permissions.Bina.View, PermissionName = "Görüntüleme", IsSelected = activePermissionValues.Contains(Permissions.Bina.View) },
                        new() { PermissionValue = Permissions.Bina.Create, PermissionName = "Ekleme", IsSelected = activePermissionValues.Contains(Permissions.Bina.Create) },
                        new() { PermissionValue = Permissions.Bina.Edit, PermissionName = "Düzenleme", IsSelected = activePermissionValues.Contains(Permissions.Bina.Edit) },
                        new() { PermissionValue = Permissions.Bina.Delete, PermissionName = "Silme", IsSelected = activePermissionValues.Contains(Permissions.Bina.Delete) }
                    }
                },
                new ModulePermissionGroup
                {
                    ModuleName = "Birim Yönetimi",
                    Permissions = new List<PermissionItem>
                    {
                        new() { PermissionValue = Permissions.Birim.View, PermissionName = "Görüntüleme", IsSelected = activePermissionValues.Contains(Permissions.Birim.View) },
                        new() { PermissionValue = Permissions.Birim.Create, PermissionName = "Ekleme", IsSelected = activePermissionValues.Contains(Permissions.Birim.Create) },
                        new() { PermissionValue = Permissions.Birim.Edit, PermissionName = "Düzenleme", IsSelected = activePermissionValues.Contains(Permissions.Birim.Edit) },
                        new() { PermissionValue = Permissions.Birim.Delete, PermissionName = "Silme", IsSelected = activePermissionValues.Contains(Permissions.Birim.Delete) }
                    }
                },
                new ModulePermissionGroup
                {
                    ModuleName = "IP Yönetimi",
                    Permissions = new List<PermissionItem>
                    {
                        new() { PermissionValue = Permissions.Ip.View, PermissionName = "Görüntüleme", IsSelected = activePermissionValues.Contains(Permissions.Ip.View) },
                        new() { PermissionValue = Permissions.Ip.Create, PermissionName = "Ekleme", IsSelected = activePermissionValues.Contains(Permissions.Ip.Create) },
                        new() { PermissionValue = Permissions.Ip.Edit, PermissionName = "Düzenleme", IsSelected = activePermissionValues.Contains(Permissions.Ip.Edit) },
                        new() { PermissionValue = Permissions.Ip.Delete, PermissionName = "Silme", IsSelected = activePermissionValues.Contains(Permissions.Ip.Delete) }
                    }
                },
                new ModulePermissionGroup
                {
                    ModuleName = "Personel Yönetimi",
                    Permissions = new List<PermissionItem>
                    {
                        new() { PermissionValue = Permissions.Person.View, PermissionName = "Görüntüleme", IsSelected = activePermissionValues.Contains(Permissions.Person.View) },
                        new() { PermissionValue = Permissions.Person.Create, PermissionName = "Ekleme", IsSelected = activePermissionValues.Contains(Permissions.Person.Create) },
                        new() { PermissionValue = Permissions.Person.Edit, PermissionName = "Düzenleme", IsSelected = activePermissionValues.Contains(Permissions.Person.Edit) },
                        new() { PermissionValue = Permissions.Person.Delete, PermissionName = "Silme", IsSelected = activePermissionValues.Contains(Permissions.Person.Delete) }
                    }
                },
                new ModulePermissionGroup
                {
                    ModuleName = "Rol & Yetki Yönetimi",
                    Permissions = new List<PermissionItem>
                    {
                        new() { PermissionValue = Permissions.Role.Manage, PermissionName = "Rol & Yetki Yönetimi", IsSelected = activePermissionValues.Contains(Permissions.Role.Manage) }
                    }
                }
            };
        }
    }
}
