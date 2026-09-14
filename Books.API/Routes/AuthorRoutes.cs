using Books.API.Models;
using Books.API.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;

namespace Books.API.Routes
{
    public static class AuthorRoutes
    {
        public static WebApplication MapAuthorEndpoints(this WebApplication app)
        {
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
            }).WithTags("Authors").CacheOutput(builder => builder.Tag("tag-author"));

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

            app.MapPost("/authors", [Authorize] async (IOutputCacheStore cache, HttpRequest request, IAuthorsRepository repository) =>
            {
                try
                {
                    using var reader = new StreamReader(request.Body);
                    var authorJson = await reader.ReadToEndAsync();

                    var author = Author.FromJson(authorJson);
                    await repository.AddAsync(author);

                    await cache.EvictByTagAsync("tag-author", default);
                    return Results.Created($"/authors/{author.Id}", author);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Authors");

            app.MapPut("/authors/{id:guid}", [Authorize] async (IOutputCacheStore cache, HttpRequest request, IAuthorsRepository repository, Guid id) =>
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

                    await cache.EvictByTagAsync("tag-author", default);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Authors");

            app.MapDelete("/authors/{id:guid}", [Authorize] async (IOutputCacheStore cache, IAuthorsRepository repository, Guid id) =>
            {
                try
                {
                    var existingAuthor = await repository.GetByIdAsync(id);

                    if (existingAuthor is null)
                    {
                        return Results.NotFound();
                    }

                    await repository.DeleteAsync(id);

                    await cache.EvictByTagAsync("tag-author", default);
                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            }).WithTags("Authors");

            return app;
        }
    }
}
