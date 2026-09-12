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
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
#endregion

var app = builder.Build();

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

#region Map the endpoints for the Books API
app.MapGet("/books", async (IBooksRepository repository, int page = 1, int pageSize = 10) =>
{
    var books = await repository.GetAllAsync(page, pageSize);

    return Results.Ok(books);
})
.WithName("GetBooks");

app.MapGet("/books/{id}", async (IBooksRepository repository, Guid id) =>
{
    var book = await repository.GetByIdAsync(id);

    if (book is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(book);
})
.WithName("GetBook");

app.MapPost("/books", async (IBooksRepository repository, Book book) =>
{
    book.Id = Guid.NewGuid();
    await repository.AddAsync(book);

    return Results.Created($"/books/{book.Id}", book);
})
.WithName("CreateBook");

app.MapPut("/books/{id}", async (IBooksRepository repository, Guid id, Book updatedBook) =>
{
    var existingBook = await repository.GetByIdAsync(id);

    if (existingBook is null)
    {
        return Results.NotFound();
    }

    updatedBook.Id = id;
    await repository.UpdateAsync(updatedBook);

    return Results.NoContent();
})
.WithName("UpdateBook");

app.MapDelete("/books/{id}", async (IBooksRepository repository, Guid id) =>
{
    bool exists = await repository.ExistsAsync(id);

    if (!exists)
    {
        return Results.NotFound();
    }

    await repository.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteBook");
#endregion

#region Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

app.Run();
