using projekt.NET.Models.Entities;


namespace projekt.NET.Repositories.Interface
{
    public interface IPlatformRepository
    {
        Task<IEnumerable<Platform>> GetAllAsync();
        Task<Platform?> GetByIdAsync(int id);
        Task AddAsync(Platform platform);
        Task UpdateAsync(Platform platform);
        Task DeleteAsync(int id);
    }
}
