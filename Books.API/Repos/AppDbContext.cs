using Books.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Repos
{
    public class AppDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
