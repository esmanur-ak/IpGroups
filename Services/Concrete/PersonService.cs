using IpGroups.Data.DbContext;
using IpGroups.Models.Entities;
using IpGroups.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace IpGroups.Services.Concrete
{
    public class PersonService : BaseService<Person>, IPersonService
    {
        public PersonService(AppDbContext context) : base(context)
        {
        }

        // DRY: Include zincirini tek bir yerde tanımlıyoruz.
        private IQueryable<Person> PersonsWithDetails => _dbSet
            .AsNoTracking()
            .Include(p => p.Bina)
            .Include(p => p.Birim)
            .Include(p => p.IpAddress)
                .ThenInclude(ia => ia!.IpGroup);

        public async Task<List<Person>> GetAllWithDetailsAsync()
        {
            var list = await PersonsWithDetails.ToListAsync();

            var personsToUpdate = new List<Person>();
            foreach (var p in list)
            {
                // Eğer IP adresinin ait olduğu IP Grubu silinmişse veya IpAddress silinmişse bağlantıyı kopar
                if (p.IpAddress != null && (p.IpAddress.IsDeleted || p.IpAddress.IpGroup == null))
                {
                    p.IpAddress = null;
                    p.IpAddressId = null;
                    
                    var dbPerson = await _context.Personeller.FindAsync(p.Id);
                    if (dbPerson != null && dbPerson.IpAddressId != null)
                    {
                        dbPerson.IpAddressId = null;
                        personsToUpdate.Add(dbPerson);
                    }
                }
            }

            if (personsToUpdate.Any())
            {
                await _context.SaveChangesAsync();
            }

            return list;
        }

        public async Task<Person?> GetByIdWithDetailsAsync(int id)
        {
            var person = await PersonsWithDetails.FirstOrDefaultAsync(p => p.Id == id);
            if (person?.IpAddress != null && (person.IpAddress.IsDeleted || person.IpAddress.IpGroup == null))
            {
                person.IpAddress = null;
                person.IpAddressId = null;
            }
            return person;
        }

        public override async Task AddAsync(Person entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.IpAddressId.HasValue)
            {
                var isIpAssigned = await _context.Personeller.AnyAsync(p => p.IpAddressId == entity.IpAddressId);
                if (isIpAssigned)
                {
                    throw new InvalidOperationException("Bu IP adresi zaten başka bir personele atanmış.");
                }

                var ipAddress = await _context.IpAdresler.FindAsync(entity.IpAddressId.Value);
                if (ipAddress != null)
                {
                    ipAddress.IsAssigned = true;
                }
            }

            await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Person entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.IpAddressId.HasValue)
            {
                var isIpAssigned = await _context.Personeller.AnyAsync(p => p.IpAddressId == entity.IpAddressId && p.Id != entity.Id);
                if (isIpAssigned)
                {
                    throw new InvalidOperationException("Bu IP adresi zaten başka bir personele atanmış.");
                }
            }

            var existingPerson = await _context.Personeller.AsNoTracking().FirstOrDefaultAsync(p => p.Id == entity.Id);
            if (existingPerson != null && existingPerson.IpAddressId != entity.IpAddressId)
            {
                if (existingPerson.IpAddressId.HasValue)
                {
                    var oldIp = await _context.IpAdresler.FindAsync(existingPerson.IpAddressId.Value);
                    if (oldIp != null)
                    {
                        oldIp.IsAssigned = false;
                        _context.IpAdresler.Update(oldIp);
                    }
                }

                if (entity.IpAddressId.HasValue)
                {
                    var newIp = await _context.IpAdresler.FindAsync(entity.IpAddressId.Value);
                    if (newIp != null)
                    {
                        newIp.IsAssigned = true;
                        _context.IpAdresler.Update(newIp);
                    }
                }
            }

            await base.UpdateAsync(entity);
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var person = await _context.Personeller.FindAsync(id);
            if (person == null)
            {
                return false;
            }

            if (person.IpAddressId.HasValue)
            {
                var ipAddress = await _context.IpAdresler.FindAsync(person.IpAddressId.Value);
                if (ipAddress != null)
                {
                    ipAddress.IsAssigned = false;
                    _context.IpAdresler.Update(ipAddress);
                }
            }

            return await base.DeleteAsync(id);
        }
    }
}