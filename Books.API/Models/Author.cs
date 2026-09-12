using System.ComponentModel.DataAnnotations;

namespace Books.API.Models
{
    public class Author
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Book> Books { get; } = [];
    }
}
