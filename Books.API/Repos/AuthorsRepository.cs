using Books.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Repos
{
    public class AuthorsRepository : IAuthorsRepository
    {
        private bool _disposed = false;

        private readonly AppDbContext _context;

        public AuthorsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Author>> GetAllAsync(int page, int pageSize)
        {
            return await _context
                            .Authors
                            .AsNoTracking()
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        }

        public async Task<Author?> GetByIdAsync(Guid id)
        {
            return await _context
                            .Authors
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Author author)
        {
            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var author = await _context.Authors.FindAsync(id) ?? throw new KeyNotFoundException($"Author with id {id} not found.");

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}