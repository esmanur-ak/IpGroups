using IpGroups.Data.DbContext;
using IpGroups.Models.Entities;
using IpGroups.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace IpGroups.Services.Concrete
{
    public class IpAddressService : BaseService<IpAddress>, IIpAddressService
    {
        public IpAddressService(AppDbContext context) : base(context)
        {
        }

        public override async Task<List<IpAddress>> GetAllAsync(bool trackChanges = false)
        {
            // Sadece aktif IP gruplarına ait, silinmemiş IP adreslerini döndür
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            return await query
                .Include(ia => ia.IpGroup)
                .Where(ia => !ia.IsDeleted && ia.IpGroup != null && !ia.IpGroup.IsDeleted)
                .ToListAsync();
        }

        public async Task SyncAllIpAddressesAsync()
        {
            try
            {
                // 1. Silinmiş IP gruplarının alt adreslerini de IsDeleted = true yap
                var deletedGroupIds = await _context.Ipler
                    .IgnoreQueryFilters()
                    .Where(ip => ip.IsDeleted)
                    .Select(ip => ip.Id)
                    .ToListAsync();

                if (deletedGroupIds.Any())
                {
                    var orphanedChildAddresses = await _context.IpAdresler
                        .Where(ia => deletedGroupIds.Contains(ia.IpGroupId) && !ia.IsDeleted)
                        .ToListAsync();

                    foreach (var addr in orphanedChildAddresses)
                    {
                        addr.IsDeleted = true;
                        addr.IsAssigned = false;
                    }
                    await _context.SaveChangesAsync();
                }

                // 2. Aktif (IsDeleted = false) olan tüm IP Gruplarını getir
                var activeIpGroups = await _context.Ipler
                    .Where(ip => !ip.IsDeleted)
                    .OrderBy(ip => ip.Id)
                    .ToListAsync();

                // Çift eklenmiş aynı IpNo varsa benzersizleştir
                var distinctGroups = activeIpGroups
                    .GroupBy(g => g.IpNo)
                    .Select(g => g.Last())
                    .ToList();

                foreach (var ipGroup in distinctGroups)
                {
                    if (ipGroup.CreatedDate <= new DateTime(2000, 1, 1))
                    {
                        ipGroup.CreatedDate = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(ipGroup.CreatedBy))
                            ipGroup.CreatedBy = "System";
                    }

                    var parts = ipGroup.IpNo.Split('.');
                    if (parts.Length != 4) continue;
                    var baseIp = $"{parts[0]}.{parts[1]}.{parts[2]}";

                    // Çakışabilecek DİĞER gruplara ait veya soft-deleted olan aynı IP adreslerini temizle
                    var conflictingOtherAddresses = await _context.IpAdresler
                        .IgnoreQueryFilters()
                        .Where(ia => ia.IpGroupId != ipGroup.Id && ia.FullIpAddress.StartsWith(baseIp + "."))
                        .ToListAsync();

                    if (conflictingOtherAddresses.Any())
                    {
                        var conflictingIds = conflictingOtherAddresses.Select(c => c.Id).ToList();
                        var assignedPersons = await _context.Personeller
                            .Where(p => p.IpAddressId.HasValue && conflictingIds.Contains(p.IpAddressId.Value))
                            .ToListAsync();

                        foreach (var p in assignedPersons)
                        {
                            p.IpAddressId = null;
                        }

                        foreach (var addr in conflictingOtherAddresses)
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

                    // Bu gruba ait alt IP adreslerini getir (soft-deleted dahil)
                    var existingAddresses = await _context.IpAdresler
                        .IgnoreQueryFilters()
                        .Where(ia => ia.IpGroupId == ipGroup.Id)
                        .ToListAsync();

                    var now = DateTime.UtcNow;

                    for (int i = 0; i <= 254; i++)
                    {
                        var expectedFull = $"{baseIp}.{i}";
                        var matches = existingAddresses.Where(ia => ia.Octet == i).ToList();

                        if (!matches.Any())
                        {
                            // Eksik ise yeni ekle
                            var newAddr = new IpAddress
                            {
                                IpGroupId = ipGroup.Id,
                                FullIpAddress = expectedFull,
                                Octet = i,
                                IsAssigned = false,
                                CreatedBy = "System",
                                CreatedDate = now
                            };
                            _context.IpAdresler.Add(newAddr);
                        }
                        else
                        {
                            // İlkini koru ve güncelle
                            var primary = matches.First();
                            if (primary.FullIpAddress != expectedFull || primary.IsDeleted)
                            {
                                primary.FullIpAddress = expectedFull;
                                primary.IsDeleted = false;
                            }

                            // Aynı octet'ten birden fazla varsa fazlalıkları sil
                            if (matches.Count > 1)
                            {
                                var duplicates = matches.Skip(1).ToList();
                                _context.IpAdresler.RemoveRange(duplicates);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                // 3. Personellerde silinmiş veya geçersiz kalmış IP atamalarını temizle
                var orphanedPersons = await _context.Personeller
                    .Include(p => p.IpAddress)
                        .ThenInclude(ia => ia!.IpGroup)
                    .Where(p => p.IpAddressId.HasValue)
                    .ToListAsync();

                var personsUpdated = false;
                foreach (var p in orphanedPersons)
                {
                    if (p.IpAddress == null || p.IpAddress.IsDeleted || p.IpAddress.IpGroup == null || p.IpAddress.IpGroup.IsDeleted)
                    {
                        p.IpAddressId = null;
                        personsUpdated = true;
                    }
                }

                if (personsUpdated)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Hata durumunda ChangeTracker'ı temizleyerek uygulamanın çökmesini engelle
                _context.ChangeTracker.Clear();
            }
        }

        public async Task<List<IpAddress>> GetByIpGroupIdAsync(int ipGroupId)
        {
            var addresses = await _dbSet
                .AsNoTracking()
                .Where(ia => ia.IpGroupId == ipGroupId && !ia.IsDeleted)
                .OrderBy(ia => ia.Octet)
                .ToListAsync();

            if (!addresses.Any())
            {
                var ipGroup = await _context.Ipler.FindAsync(ipGroupId);
                if (ipGroup != null)
                {
                    var ipParts = ipGroup.IpNo.Split('.');
                    if (ipParts.Length == 4)
                    {
                        // Eşzamanlı isteklerde çift eklemeyi önlemek için tekrar kontrol et
                        bool alreadyExists = await _context.IpAdresler
                            .AnyAsync(ia => ia.IpGroupId == ipGroupId);

                        if (!alreadyExists)
                        {
                            var baseIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";
                            var newAddresses = new List<IpAddress>();
                            var now = DateTime.UtcNow;
                            for (int i = 0; i <= 254; i++)
                            {
                                newAddresses.Add(new IpAddress
                                {
                                    IpGroupId = ipGroupId,
                                    FullIpAddress = $"{baseIp}.{i}",
                                    Octet = i,
                                    IsAssigned = false,
                                    CreatedBy = "System",
                                    CreatedDate = now,
                                    DeletedBy = string.Empty
                                });
                            }

                            try
                            {
                                await _context.IpAdresler.AddRangeAsync(newAddresses);
                                await _context.SaveChangesAsync();
                                addresses = newAddresses;
                            }
                            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                            {
                                // Başka bir istek daha önce eklemiş olabilir, mevcut kayıtları getir
                                _context.ChangeTracker.Clear();
                                addresses = await _dbSet
                                    .AsNoTracking()
                                    .Where(ia => ia.IpGroupId == ipGroupId && !ia.IsDeleted)
                                    .OrderBy(ia => ia.Octet)
                                    .ToListAsync();
                            }
                        }
                        else
                        {
                            // Soft-deleted kayıtlar varsa hepsini getir
                            addresses = await _dbSet
                                .AsNoTracking()
                                .Where(ia => ia.IpGroupId == ipGroupId && !ia.IsDeleted)
                                .OrderBy(ia => ia.Octet)
                                .ToListAsync();
                        }
                    }
                }
            }

            return addresses;
        }

        public async Task<List<IpAddress>> GetUnassignedByGroupIdAsync(int ipGroupId)
        {
            var all = await GetByIpGroupIdAsync(ipGroupId);
            return all.Where(ia => !ia.IsAssigned).OrderBy(ia => ia.Octet).ToList();
        }
    }
}
