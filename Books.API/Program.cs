using Books.API.Models;
using Books.API.Repos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add services
// Configure MS SQL Server
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the repository
builder
    .Services
    .AddScoped<IAuthorsRepository, AuthorsRepository>()
    .AddScoped<IBooksRepository, BooksRepository>();
#endregion

var app = builder.Build();

#region Set up the app
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Add Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();
#endregion

#region Map the endpoints for Authors
app.MapGet("/authors", async (IAuthorsRepository repository, int page = 1, int pageSize = 10) =>
{
    if (page <= 0 || pageSize <= 0)
        return Results.BadRequest("Params page and pageSize must be greater than 0.");

    try
    {
        var authors = await repository.GetAllAsync(page, pageSize);

        return Results.Ok(authors);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithTags("Authors");

app.MapGet("/authors/{id:guid}", async (IAuthorsRepository repository, Guid id) =>
{
    try
    {
        var author = await repository.GetByIdAsync(id);

        if (author is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(author);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithTags("Authors");

app.MapPost("/authors", async (HttpRequest request, IAuthorsRepository repository) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var authorJson = await reader.ReadToEndAsync();

        var author = Author.FromJson(authorJson);
        await repository.AddAsync(author);

        return Results.Created($"/authors/{author.Id}", author);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithTags("Authors");

app.MapPut("/authors/{id:guid}", async (HttpRequest request, IAuthorsRepository repository, Guid id) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var updatedAuthorJson = await reader.ReadToEndAsync();

        var updatedAuthor = Author.FromJson(updatedAuthorJson, throwExceptionIfInvalid: false);
        var existingAuthor = await repository.GetByIdAsync(id);

        if (existingAuthor is null)
        {
            return Results.NotFound();
        }

        existingAuthor.UpdateFrom(updatedAuthor);
        await repository.UpdateAsync(existingAuthor);

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithTags("Authors");

app.MapDelete("/authors/{id:guid}", async (IAuthorsRepository repository, Guid id) =>
{
    try
    {
        var existingAuthor = await repository.GetByIdAsync(id);

        if (existingAuthor is null)
        {
            return Results.NotFound();
        }

        await repository.DeleteAsync(id);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithTags("Authors");
#endregion

#region Map the endpoints for Books
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


app.MapPost("/authors/{authorId:guid}/books", async (HttpRequest request, IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId) =>
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

app.MapPut("/authors/{authorId:guid}/books/{id}", async (HttpRequest request, IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, Guid id) =>
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

app.MapDelete("/authors/{authorId:guid}/books/{id}", async (IAuthorsRepository authorsRepository, IBooksRepository booksRepository, Guid authorId, Guid id) =>
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
#endregion

#region Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

app.Run();
