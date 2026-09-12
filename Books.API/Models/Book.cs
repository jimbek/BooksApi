using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

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

        public void UpdateFrom(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            AuthorId = book.AuthorId;
            Title = book.Title;
            Description = book.Description;
        }

        public static Book FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string cannot be null or whitespace.");

            var bookNode = JsonNode.Parse(json)!;

            return new Book
            {
                Id = bookNode["id"]?.GetValue<Guid>() ?? Guid.NewGuid(),
                AuthorId = bookNode["authorId"]?.GetValue<Guid>() ?? Guid.Empty,
                Title = bookNode["title"]?.GetValue<string>() ?? string.Empty,
                Description = bookNode["description"]?.GetValue<string>() ?? string.Empty
            };
        }
    }
}
