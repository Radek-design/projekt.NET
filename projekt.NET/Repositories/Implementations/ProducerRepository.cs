using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models.Entities;
using projekt.NET.Repositories.Interface;


namespace projekt.NET.Repositories.Implementations
{
    public class ProducerRepository : IProducerRepository
    {
        private readonly AppDbContext _context;

        public ProducerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producer>> GetAllAsync()
        {
            return await _context.Producers.ToListAsync();
        }

        public async Task<Producer?> GetByIdAsync(int id)
        {
            return await _context.Producers
                .Include(p => p.Games)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Producer producer)
        {
            await _context.Producers.AddAsync(producer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Producer producer)
        {
            _context.Producers.Update(producer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var producer = await GetByIdAsync(id);
            if (producer != null)
            {
                _context.Producers.Remove(producer);
                await _context.SaveChangesAsync();
            }
        }
    }
}