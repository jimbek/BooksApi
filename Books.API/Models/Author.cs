using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace Books.API.Models
{
    public class Author
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        public ICollection<Book> Books { get; } = [];

        public void UpdateFrom(Author author)
        {
            ArgumentNullException.ThrowIfNull(author);

            Name = author.Name ?? Name;
        }

        public static Author FromJson(string json, bool throwExceptionIfInvalid = true)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string cannot be null or whitespace.");

            JsonNode authorNode = JsonNode.Parse(json)!;

            return new Author
            {
                Id = authorNode["id"]?.GetValue<Guid>() ?? Guid.NewGuid(),
                Name = authorNode["name"]?.GetValue<string>() ?? (throwExceptionIfInvalid ? throw new ArgumentException("Invalid JSON: missing 'name'") : null)
            };
        }
    }
}
