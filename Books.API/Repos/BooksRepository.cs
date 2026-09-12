using Books.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Repos
{
    public class BooksRepository : IBooksRepository
    {
        private readonly AppDbContext _context;

        public BooksRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Books.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Books.AsNoTracking().AnyAsync(b => b.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id) ?? throw new KeyNotFoundException($"Book with Id {id} was not found.");
            
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }
}
