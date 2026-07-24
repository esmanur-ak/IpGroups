using IpGroups.Data.DbContext;
using IpGroups.Models.Entities;
using IpGroups.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace IpGroups.Services.Concrete
{
    public class IpService : BaseService<Ip>, IIpService
    {
        public IpService(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Ip>> GetUnassignedIpsAsync()
        {
            // Optimize edilmiş sorgu: Tek bir SQL EXISTS / NOT EXISTS sorgusu ile boşta olan IP'leri çeker.
            return await _dbSet
                .AsNoTracking()
                .Where(ip => !_context.Personeller.Any(p => p.IpAddressId == ip.Id))
                .ToListAsync();
        }

        public async Task<List<Ip>> GetAllWithAddressesAsync()
        {
            return await _dbSet
                .Include(ip => ip.IpAddresses)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task AddAsync(Ip entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            // 1. Veritabanında (soft-delete dahil) aynı IpNo var mı kontrol et
            var existingIpGroup = await _context.Ipler
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(ip => ip.IpNo == entity.IpNo);

            if (existingIpGroup != null)
            {
                if (!existingIpGroup.IsDeleted)
                {
                    throw new InvalidOperationException("Bu IP adresi zaten kayıtlı.");
                }

                // Önceden silinmiş bu IP grubunu ve alt adreslerini yeniden aktif et
                existingIpGroup.IsDeleted = false;
                existingIpGroup.Description = entity.Description;
                existingIpGroup.CreatedDate = DateTime.UtcNow;
                existingIpGroup.CreatedBy = string.IsNullOrEmpty(entity.CreatedBy) ? "System" : entity.CreatedBy;
                existingIpGroup.UpdatedDate = null;
                existingIpGroup.UpdatedBy = null;
                existingIpGroup.DeletedDate = null;
                existingIpGroup.DeletedBy = string.Empty;

                var restoreParts = existingIpGroup.IpNo.Split('.');
                if (restoreParts.Length == 4)
                {
                    var baseIp = $"{restoreParts[0]}.{restoreParts[1]}.{restoreParts[2]}";

                    // Başka gruplara ait çakışan IP adreslerinin FullIpAddress alanlarını serbest bırak
                    await FreeUpFullIpAddressesAsync(baseIp, existingIpGroup.Id);

                    // Mevcut alt adresleri getir ve güncelle
                    var childAddresses = await _context.IpAdresler
                        .IgnoreQueryFilters()
                        .Where(ia => ia.IpGroupId == existingIpGroup.Id)
                        .ToListAsync();

                    foreach (var child in childAddresses)
                    {
                        child.IsDeleted = false;
                        child.IsAssigned = false;
                        child.FullIpAddress = $"{baseIp}.{child.Octet}";
                    }

                    var existingOctets = childAddresses.Select(c => c.Octet).ToHashSet();

                    for (int i = 0; i <= 254; i++)
                    {
                        if (!existingOctets.Contains(i))
                        {
                            _context.IpAdresler.Add(new IpAddress
                            {
                                IpGroupId = existingIpGroup.Id,
                                FullIpAddress = $"{baseIp}.{i}",
                                Octet = i,
                                IsAssigned = false,
                                IsDeleted = false,
                                CreatedBy = "System",
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }

                entity.Id = existingIpGroup.Id;
                await _context.SaveChangesAsync();
                return;
            }

            // 2. Tamamen yeni bir IP grubu eklenecek
            var ipParts = entity.IpNo.Split('.');
            if (ipParts.Length == 4)
            {
                var baseIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";

                // Eğer başka bir grubun eski soft-deleted alt IP adresleri ile çakışıyorsa serbest bırak
                await FreeUpFullIpAddressesAsync(baseIp);
            }

            // IP grubunu kaydet
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            // 0-254 arası alt IP adreslerini otomatik oluştur
            if (ipParts.Length == 4)
            {
                var baseIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";

                var ipAddresses = new List<IpAddress>();
                for (int i = 0; i <= 254; i++)
                {
                    ipAddresses.Add(new IpAddress
                    {
                        IpGroupId = entity.Id,
                        FullIpAddress = $"{baseIp}.{i}",
                        Octet = i,
                        IsAssigned = false
                    });
                }

                await _context.IpAdresler.AddRangeAsync(ipAddresses);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task UpdateAsync(Ip entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var exists = await _context.Ipler
                .IgnoreQueryFilters()
                .AnyAsync(ip => ip.IpNo == entity.IpNo && ip.Id != entity.Id && !ip.IsDeleted);

            if (exists)
            {
                throw new InvalidOperationException("Bu IP adresi zaten kayıtlı.");
            }

            // Mevcut IP grubunun eski IpNo değerini al
            var existingIp = await _dbSet.AsNoTracking().FirstOrDefaultAsync(ip => ip.Id == entity.Id);
            if (existingIp != null && existingIp.IpNo != entity.IpNo)
            {
                // IP grubu değiştiğinde, alt IP adreslerini de güncelle
                var newParts = entity.IpNo.Split('.');
                if (newParts.Length == 4)
                {
                    var newBaseIp = $"{newParts[0]}.{newParts[1]}.{newParts[2]}";

                    // Çakışabilecek IP adreslerini serbest bırak
                    await FreeUpFullIpAddressesAsync(newBaseIp, entity.Id);

                    var childAddresses = await _context.IpAdresler
                        .Where(ia => ia.IpGroupId == entity.Id)
                        .ToListAsync();

                    foreach (var addr in childAddresses)
                    {
                        addr.FullIpAddress = $"{newBaseIp}.{addr.Octet}";
                    }
                }
            }

            // IP grubunu güncelle
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            // Alt IP adreslerini de soft-delete yap ve atanan personellerin IP bağlantısını kaldır
            var childAddresses = await _context.IpAdresler
                .Where(ia => ia.IpGroupId == id)
                .ToListAsync();

            if (childAddresses.Any())
            {
                var childIds = childAddresses.Select(ia => ia.Id).ToList();

                var assignedPersons = await _context.Personeller
                    .Where(p => p.IpAddressId.HasValue && childIds.Contains(p.IpAddressId.Value))
                    .ToListAsync();

                foreach (var p in assignedPersons)
                {
                    p.IpAddressId = null;
                }

                foreach (var addr in childAddresses)
                {
                    addr.IsAssigned = false;
                }

                _context.IpAdresler.RemoveRange(childAddresses);
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task FreeUpFullIpAddressesAsync(string baseIp, int? keepGroupId = null)
        {
            var conflicting = await _context.IpAdresler
                .IgnoreQueryFilters()
                .Where(ia => ia.FullIpAddress.StartsWith(baseIp + ".") && (keepGroupId == null || ia.IpGroupId != keepGroupId.Value))
                .ToListAsync();

            if (conflicting.Any())
            {
                var conflictingIds = conflicting.Select(c => c.Id).ToList();
                var assignedPersons = await _context.Personeller
                    .Where(p => p.IpAddressId.HasValue && conflictingIds.Contains(p.IpAddressId.Value))
                    .ToListAsync();

                foreach (var p in assignedPersons)
                {
                    p.IpAddressId = null;
                }

                foreach (var addr in conflicting)
                {
                    if (!addr.FullIpAddress.StartsWith("OLD_"))
                    {
                        addr.FullIpAddress = $"OLD_{addr.Id}_{addr.FullIpAddress}";
                    }
                    addr.IsDeleted = true;
                    addr.IsAssigned = false;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
