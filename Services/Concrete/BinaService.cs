using IpGroups.Data.DbContext;
using IpGroups.Models.Entities;
using IpGroups.Services.Abstract;

using Microsoft.EntityFrameworkCore;

namespace IpGroups.Services.Concrete
{
    public class BinaService : BaseService<Bina>, IBinaService
    {
        public BinaService(AppDbContext context) : base(context)
        {
        }

        public override async Task AddAsync(Bina entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var exists = await _dbSet.AnyAsync(b => b.Name.ToLower() == entity.Name.ToLower());
            if (exists)
            {
                throw new InvalidOperationException("Bu bina adı zaten kayıtlı.");
            }

            await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Bina entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var exists = await _dbSet.AnyAsync(b => b.Name.ToLower() == entity.Name.ToLower() && b.Id != entity.Id);
            if (exists)
            {
                throw new InvalidOperationException("Bu bina adı zaten kayıtlı.");
            }

            await base.UpdateAsync(entity);
        }
    }
}