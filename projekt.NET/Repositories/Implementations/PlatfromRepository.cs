using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models.Entities;
using projekt.NET.Repositories.Interface;

namespace projekt.NET.Repositories.Implementations
{
    public class PlatformRepository : IPlatformRepository
    {
        private readonly AppDbContext _context;

        public PlatformRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Platform>> GetAllAsync()
        {
            return await _context.Platforms.ToListAsync();
        }

        public async Task<Platform?> GetByIdAsync(int id)
        {
            return await _context.Platforms.FindAsync(id);
        }

        public async Task AddAsync(Platform platform)
        {
            await _context.Platforms.AddAsync(platform);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Platform platform)
        {
            _context.Platforms.Update(platform);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var platform = await GetByIdAsync(id);
            if (platform != null)
            {
                _context.Platforms.Remove(platform);
                await _context.SaveChangesAsync();
            }
        }
    }
}