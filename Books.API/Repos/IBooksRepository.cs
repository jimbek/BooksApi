using Books.API.Models;

namespace Books.API.Repos
{
    public interface IBooksRepository : IDisposable
    {
        Task<IEnumerable<Book>> GetAllAsync(Guid authorId, int page, int pageSize);

        Task<Book?> GetByIdAsync(Guid id);

        Task AddAsync(Book book);

        Task UpdateAsync(Book book);

        Task DeleteAsync(Guid id);
    }
}
