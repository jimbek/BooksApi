using Books.API.Models;
using Books.API.Repos;
using Microsoft.AspNetCore.Authorization;

namespace Books.API.Routes
{
    public static class BookRoutes
    {
        public static WebApplication MapBookEndpoints(this WebApplication app)
        {
            app.MapGet("/authors/{authorId:guid}/books", async (IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, int page = 1, int pageSize = 10) =>
            {
                try
                {
                    if (page <= 0 || pageSize <= 0)
                        return Results.BadRequest("Params page and pageSize must be greater than 0.");

                    // Check if the author exists
                    var author = await authorsRepository.GetByIdAsync(authorId);

                    if (author is null)
                    {
                        return Results.NotFound($"Author with ID {authorId} not found.");
                    }

                    var books = await booksRepository.GetAllAsync(authorId, page, pageSize);

                    return Results.Ok(books);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Books");

            app.MapGet("/authors/{authorId:guid}/books/{id}", async (IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, Guid id) =>
            {
                try
                {
                    // Check if the author exists
                    var author = await authorsRepository.GetByIdAsync(authorId);

                    if (author is null)
                    {
                        return Results.NotFound($"Author with ID {authorId} not found.");
                    }

                    var book = await booksRepository.GetByIdAsync(id);

                    if (book is null || book.AuthorId != authorId)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(book);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Books");


            app.MapPost("/authors/{authorId:guid}/books", [Authorize] async (HttpRequest request, IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId) =>
            {
                try
                {
                    // Check if the author exists
                    var author = await authorsRepository.GetByIdAsync(authorId);

                    if (author is null)
                    {
                        return Results.NotFound($"Author with ID {authorId} not found.");
                    }

                    using var reader = new StreamReader(request.Body);
                    var bookJson = await reader.ReadToEndAsync();

                    var book = Book.FromJson(authorId, bookJson);
                    await booksRepository.AddAsync(book);

                    return Results.Created($"/author/{authorId}/books/{book.Id}", book);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Books");

            app.MapPut("/authors/{authorId:guid}/books/{id}", [Authorize] async (HttpRequest request, IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, Guid id) =>
            {
                try
                {
                    // Check if the author exists
                    var author = await authorsRepository.GetByIdAsync(authorId);

                    if (author is null)
                    {
                        return Results.NotFound($"Author with ID {authorId} not found.");
                    }

                    using var reader = new StreamReader(request.Body);
                    var bookJson = await reader.ReadToEndAsync();

                    var updatedBook = Book.FromJson(authorId, bookJson, throwExceptionIfInvalid: false);
                    var existingBook = await booksRepository.GetByIdAsync(id);

                    if (existingBook is null || existingBook.AuthorId != authorId)

                    {
                        return Results.NotFound();
                    }

                    existingBook.UpdateFrom(updatedBook);
                    await booksRepository.UpdateAsync(existingBook);

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Books");

            app.MapDelete("/authors/{authorId:guid}/books/{id}", [Authorize] async (IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, Guid id) =>
            {
                try
                {
                    // Check if the author exists
                    var author = await authorsRepository.GetByIdAsync(authorId);

                    if (author is null)
                    {
                        return Results.NotFound($"Author with ID {authorId} not found.");
                    }

                    var existingBook = await booksRepository.GetByIdAsync(id);

                    if (existingBook is null || existingBook.AuthorId != authorId)
                    {
                        return Results.NotFound();
                    }

                    await booksRepository.DeleteAsync(id);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Books");

            return app;
        }
    }
}
