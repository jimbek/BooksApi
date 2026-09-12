using Books.API.Models;

namespace Books.API.Repos
{
    public interface IBooksRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(int page, int pageSize);

        Task<Book?> GetByIdAsync(Guid id);

        Task<bool> ExistsAsync(Guid id);

        Task AddAsync(Book book);

        Task UpdateAsync(Book book);

        Task DeleteAsync(Guid id);
    }
}
