using IpGroups.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IpGroups.Data.DbContext
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Bina> Binalar { get; set; }
        public DbSet<Birim> Birimler { get; set; }
        public DbSet<Ip> Ipler { get; set; }
        public DbSet<IpAddress> IpAdresler { get; set; }
        public DbSet<Person> Personeller { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration dosyalarını otomatik olarak uygula
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditAndSoftDelete()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedBy = string.IsNullOrEmpty(entry.Entity.CreatedBy) ? currentUser : entry.Entity.CreatedBy;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        if (entry.Entity.CreatedDate <= new DateTime(2000, 1, 1))
                        {
                            entry.Entity.CreatedDate = now;
                            if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                                entry.Entity.CreatedBy = currentUser;
                        }

                        // Restore durumunda (IsDeleted true→false) UpdatedDate ayarlama
                        var isDeletedProp = entry.Property(nameof(BaseEntity.IsDeleted));
                        var isRestore = isDeletedProp.IsModified
                            && isDeletedProp.OriginalValue is true
                            && entry.Entity.IsDeleted == false;

                        if (!isRestore)
                        {
                            entry.Entity.UpdatedDate = now;
                            entry.Entity.UpdatedBy = currentUser;
                        }
                        break;

                    case EntityState.Deleted:
                        // Gerçek silme işlemini soft delete'e dönüştür
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedDate = now;
                        entry.Entity.DeletedBy = currentUser;
                        break;
                }
            }
        }
    }
}
