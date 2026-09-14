using Books.API.Repos;
using Books.API.Routes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

#region Add services
// Configure MS SQL Server
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Set up authorization
builder.
    Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = "https://localhost:44316",
            ValidAudience = "https://localhost:44316",
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("a-string-secret-at-least-256-bits-long"))
        };
    });

builder
    .Services
    .AddAuthorization();

// Set up output caching
builder
    .Services
    .AddOutputCache(options =>
    {
        options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
    });

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

#region Map the endpoints
app.MapAuthorEndpoints();
app.MapBookEndpoints();
#endregion

#region Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

app.Run();
