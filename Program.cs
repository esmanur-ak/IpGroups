using IpGroups.Constants;
using IpGroups.Data.DbContext;
using IpGroups.Extensions;
using IpGroups.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+çğıöşüÇĞİÖŞÜ";
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "IpGroups.Auth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Dynamic Authorization Policies for all Granular Permissions
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.GetAllPermissions())
    {
        options.AddPolicy(permission, policy => 
            policy.RequireAssertion(ctx => 
                ctx.User.IsInRole("Ana Yönetici") || 
                ctx.User.HasClaim("Permission", permission)));
    }
});

builder.Services.AddHttpContextAccessor();

// Register Business Services
builder.Services.AddBusinessServices();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Migration and Seed Data Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    // Identity Seed Data (Roles & Default Admin)
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    // Update "Admin" to "Ana Yönetici" if it exists
    var oldAdminRole = await roleManager.FindByNameAsync("Admin");
    if (oldAdminRole != null)
    {
        oldAdminRole.Name = "Ana Yönetici";
        oldAdminRole.Description = "Ana Yönetici Rolü";
        await roleManager.UpdateAsync(oldAdminRole);
    }

    string[] roles = { "Ana Yönetici", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Name = role, Description = $"{role} Rolü" });
        }
    }

    // Seed Claims for Ana Yönetici Role (All Permissions)
    var adminRole = await roleManager.FindByNameAsync("Ana Yönetici");
    if (adminRole != null)
    {
        var existingClaims = await roleManager.GetClaimsAsync(adminRole);
        foreach (var permission in Permissions.GetAllPermissions())
        {
            if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim("Permission", permission));
            }
        }
    }

    // Clear default Claims for User Role (User should have NO permissions by default)
    var userRole = await roleManager.FindByNameAsync("User");
    if (userRole != null)
    {
        var existingClaims = await roleManager.GetClaimsAsync(userRole);
        foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
        {
            await roleManager.RemoveClaimAsync(userRole, claim);
        }
    }

    var adminEmail = "admin@ipgroups.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            AdSoyad = "Sistem Yöneticisi",
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow
        };
        var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Ana Yönetici");
        }
    }
    else
    {
        // Ensure admin@ipgroups.com is in Ana Yönetici role
        if (!await userManager.IsInRoleAsync(adminUser, "Ana Yönetici"))
        {
            await userManager.AddToRoleAsync(adminUser, "Ana Yönetici");
        }
    }

    // Sync IsAssigned flag for all IP Addresses based on existing Person assignments on startup
    var assignedIpIds = await context.Personeller
        .Where(p => p.IpAddressId.HasValue)
        .Select(p => p.IpAddressId!.Value)
        .Distinct()
        .ToListAsync();

    var ipAddresses = await context.IpAdresler.ToListAsync();
    bool anyChange = false;
    foreach (var ip in ipAddresses)
    {
        bool shouldBeAssigned = assignedIpIds.Contains(ip.Id);
        if (ip.IsAssigned != shouldBeAssigned)
        {
            ip.IsAssigned = shouldBeAssigned;
            anyChange = true;
        }
    }

    if (anyChange)
    {
        await context.SaveChangesAsync();
    }
}

app.Run();
