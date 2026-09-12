using System.ComponentModel.DataAnnotations;

namespace Books.API.Models
{
    public class Book
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public Author Author { get; set; } = null!;
    }
}
