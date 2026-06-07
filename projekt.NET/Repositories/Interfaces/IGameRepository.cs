using projekt.NET.Models.Entities;

namespace projekt.NET.Repositories.Interface
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAllGAsync();
        Task<Game?> GetByIdAsync(int id);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
        Task DeleteAsync(int id);
    }
}
