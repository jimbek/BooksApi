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
        public string? Title { get; set; }

        [Required]
        [MaxLength(500)]
        public string? Description { get; set; }

        public Author Author { get; set; } = null!;

        public void UpdateFrom(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            AuthorId = book.AuthorId;

            Title = book.Title ?? Title;
            Description = book.Description ?? Description;
        }

        public static Book FromJson(Guid authorId, string json, bool throwExceptionIfInvalid = true)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string cannot be null or whitespace.");

            var bookNode = JsonNode.Parse(json)!;

            return new Book
            {
                Id = bookNode["id"]?.GetValue<Guid>() ?? Guid.NewGuid(),
                AuthorId = authorId,
                Title = bookNode["title"]?.GetValue<string>() ?? (throwExceptionIfInvalid ? throw new ArgumentException("Invalid JSON: missing 'title'") : null),
                Description = bookNode["description"]?.GetValue<string>() ?? (throwExceptionIfInvalid ? throw new ArgumentException("Invalid JSON: missing 'description'") : null)
            };
        }
    }
}
