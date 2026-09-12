using Books.API.Models;

namespace Books.API.Repos
{
    public interface IAuthorsRepository : IDisposable
    {
        Task<IEnumerable<Author>> GetAllAsync(int page, int pageSize);

        Task<Author?> GetByIdAsync(Guid id);

        Task AddAsync(Author author);

        Task UpdateAsync(Author author);

        Task DeleteAsync(Guid id);
    }
}
