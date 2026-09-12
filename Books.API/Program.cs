using Books.API.Models;
using Books.API.Repos;
using Microsoft.AspNetCore.Mvc;
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

    var authors = await repository.GetAllAsync(page, pageSize);

    return Results.Ok(authors);
}).WithTags("Authors");

app.MapGet("/authors/{id:guid}", async (IAuthorsRepository repository, Guid id) =>
{
    var author = await repository.GetByIdAsync(id);

    if (author is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(author);
}).WithTags("Authors");

app.MapPost("/authors", async (HttpRequest request, IAuthorsRepository repository) =>
{
    using var reader = new StreamReader(request.Body);
    var authorJson = await reader.ReadToEndAsync();

    var author = Author.FromJson(authorJson);
    await repository.AddAsync(author);

    return Results.Created($"/authors/{author.Id}", author);
}).WithTags("Authors");

app.MapPut("/authors/{id:guid}", async (HttpRequest request, IAuthorsRepository repository, Guid id) =>
{
    using var reader = new StreamReader(request.Body);
    var updatedAuthorJson = await reader.ReadToEndAsync();

    var updatedAuthor = Author.FromJson(updatedAuthorJson);
    var existingAuthor = await repository.GetByIdAsync(id);

    if (existingAuthor is null)
    {
        return Results.NotFound();
    }

    existingAuthor.UpdateFrom(updatedAuthor);
    await repository.UpdateAsync(existingAuthor);

    return Results.NoContent();
}).WithTags("Authors");

app.MapDelete("/authors/{id:guid}", async (IAuthorsRepository repository, Guid id) =>
{
    var existingAuthor = await repository.GetByIdAsync(id);

    if (existingAuthor is null)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(id);
    return Results.NoContent();
}).WithTags("Authors");
#endregion

#region Map the endpoints for Books
app.MapGet("/author/{authorId:guid}/books", async (IBooksRepository repository, Guid authorId, int page = 1, int pageSize = 10) =>
{
    if (page <= 0 || pageSize <= 0)
        return Results.BadRequest("Params page and pageSize must be greater than 0.");

    var books = await repository.GetAllAsync(authorId, page, pageSize);

    return Results.Ok(books);
}).WithTags("Books");

app.MapGet("/author/{authorId:guid}/books/{id}", async (IBooksRepository repository, Guid authorId, Guid id) =>
{
    var book = await repository.GetByIdAsync(id);

    if (book is null || book.AuthorId != authorId)
    {
        return Results.NotFound();
    }

    return Results.Ok(book);
}).WithTags("Books");

app.MapPost("/author/{authorId:guid}/books", async (IBooksRepository repository, Guid authorId, Book book) =>
{
    book.AuthorId = authorId;

    await repository.AddAsync(book);

    return Results.Created($"/author/{authorId}/books/{book.Id}", book);
}).WithTags("Books");

app.MapPut("/author/{authorId:guid}/books/{id}", async (IBooksRepository repository, Guid authorId, Guid id, Book updatedBook) =>
{
    var existingBook = await repository.GetByIdAsync(id);

    if (existingBook is null || existingBook.AuthorId != authorId)

    {
        return Results.NotFound();
    }

    updatedBook.Id = id;
    await repository.UpdateAsync(updatedBook);

    return Results.NoContent();
}).WithTags("Books");

app.MapDelete("/author/{authorId:guid}/books/{id}", async (IBooksRepository repository, Guid authorId, Guid id) =>
{
    var existingBook = await repository.GetByIdAsync(id);

    if (existingBook is null || existingBook.AuthorId != authorId)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(id);
    return Results.NoContent();
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
