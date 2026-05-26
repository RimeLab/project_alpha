using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<AppMetadata>(builder.Configuration.GetSection("AppMetadata"));
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (db.Database.IsRelational())
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
            {
                // Tables exist from a prior migration set — sync history without re-running DDL
#pragma warning disable EF1003
                foreach (var m in db.Database.GetPendingMigrations())
                    await db.Database.ExecuteSqlRawAsync(
                        $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") " +
                        $"VALUES ('{m}', '10.0.8') ON CONFLICT DO NOTHING");
#pragma warning restore EF1003
                break;
            }
            catch when (attempt < 10)
            {
                Console.WriteLine($"Database not ready, retrying in 3s... (attempt {attempt}/10)");
                await Task.Delay(3000);
            }
        }
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    await DbSeeder.SeedAsync(db);
}

app.MapOpenApi();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "AlphaApi v1"));

app.MapGet("/", () => new { message = "Welcome" });
app.MapGet("/metadata", (IOptions<AppMetadata> metadata) => metadata.Value);

// --- User ---

var users = app.MapGroup("/users");

users.MapPost("/", async (CreateUserRequest req, AppDbContext db) =>
{
    var user = new User
    {
        Username = req.Username,
        Password = PasswordHasher.Hash(req.Password),
        Description = req.Description
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", new { user.Id, user.Username, user.Description });
});

users.MapGet("/", async (AppDbContext db) =>
    await db.Users
        .Select(u => new { u.Id, u.Username, u.Description })
        .ToListAsync());

users.MapGet("/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user is null
        ? Results.NotFound()
        : Results.Ok(new { user.Id, user.Username, user.Description });
});

users.MapPut("/{id}", async (int id, UpdateUserRequest req, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Username = req.Username;
    if (req.Password is not null)
        user.Password = PasswordHasher.Hash(req.Password);
    user.Description = req.Description;

    await db.SaveChangesAsync();
    return Results.Ok(new { user.Id, user.Username, user.Description });
});

users.MapDelete("/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// --- Coffee ---

var coffees = app.MapGroup("/coffee");

coffees.MapPost("/", async (CreateCoffeeRequest req, AppDbContext db) =>
{
    if (req.Intensity is < 1 or > 10)
        return Results.BadRequest(new { error = "Intensity must be between 1 and 10." });
    if (req.Rating is < 1 or > 5)
        return Results.BadRequest(new { error = "Rating must be between 1 and 5." });

    if (!await db.Users.AnyAsync(u => u.Id == req.UserId))
        return Results.BadRequest(new { error = "User not found." });

    var coffee = new Coffee
    {
        Type = req.Type,
        Shop = req.Shop,
        Location = req.Location,
        Intensity = req.Intensity,
        Rating = req.Rating,
        Temperature = req.Temperature,
        Notes = req.Notes,
        UserId = req.UserId
    };
    db.Coffees.Add(coffee);
    await db.SaveChangesAsync();
    return Results.Created($"/coffee/{coffee.Id}", new
    {
        coffee.Id, coffee.Type, coffee.Shop, coffee.Location,
        coffee.Intensity, coffee.Rating, coffee.Temperature, coffee.Notes, coffee.UserId
    });
});

coffees.MapGet("/", async (AppDbContext db) =>
    await db.Coffees
        .Select(c => new { c.Id, c.Type, c.Shop, c.Location, c.Intensity, c.Rating, c.Temperature, c.Notes, c.UserId })
        .ToListAsync());

coffees.MapGet("/{id}", async (int id, AppDbContext db) =>
{
    var coffee = await db.Coffees
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (coffee is null) return Results.NotFound();

    return Results.Ok(new
    {
        coffee.Id, coffee.Type, coffee.Shop, coffee.Location,
        coffee.Intensity, coffee.Rating, coffee.Temperature, coffee.Notes,
        coffee.UserId,
        User = new { coffee.User.Id, coffee.User.Username, coffee.User.Description }
    });
});

coffees.MapPut("/{id}", async (int id, UpdateCoffeeRequest req, AppDbContext db) =>
{
    if (req.Intensity is < 1 or > 10)
        return Results.BadRequest(new { error = "Intensity must be between 1 and 10." });
    if (req.Rating is < 1 or > 5)
        return Results.BadRequest(new { error = "Rating must be between 1 and 5." });

    var coffee = await db.Coffees.FindAsync(id);
    if (coffee is null) return Results.NotFound();

    if (!await db.Users.AnyAsync(u => u.Id == req.UserId))
        return Results.BadRequest(new { error = "User not found." });

    coffee.Type = req.Type;
    coffee.Shop = req.Shop;
    coffee.Location = req.Location;
    coffee.Intensity = req.Intensity;
    coffee.Rating = req.Rating;
    coffee.Temperature = req.Temperature;
    coffee.Notes = req.Notes;
    coffee.UserId = req.UserId;

    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        coffee.Id, coffee.Type, coffee.Shop, coffee.Location,
        coffee.Intensity, coffee.Rating, coffee.Temperature, coffee.Notes, coffee.UserId
    });
});

coffees.MapDelete("/{id}", async (int id, AppDbContext db) =>
{
    var coffee = await db.Coffees.FindAsync(id);
    if (coffee is null) return Results.NotFound();

    db.Coffees.Remove(coffee);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
