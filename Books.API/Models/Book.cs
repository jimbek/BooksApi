using System.ComponentModel.DataAnnotations;

namespace Books.API.Models
{
    public class Book
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    }
}
