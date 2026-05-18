namespace AlphaApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var users = new[]
        {
            new User { Username = "frost",   Password = PasswordHasher.Hash("password123"), Description = "Coffee obsessive" },
            new User { Username = "maya",    Password = PasswordHasher.Hash("password123"), Description = "Espresso purist" },
            new User { Username = "charlie", Password = PasswordHasher.Hash("password123"), Description = "Cold brew convert" },
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var coffees = new[]
        {
            new Coffee { Type = "Espresso",    Shop = "Blue Bottle",    Location = "San Francisco, CA", Intensity = 9, Rating = 5, Temperature = "Hot",  Notes = "Nutty finish, great crema",          UserId = users[0].Id },
            new Coffee { Type = "Latte",       Shop = "Sightglass",     Location = "San Francisco, CA", Intensity = 5, Rating = 4, Temperature = "Hot",  Notes = "Smooth and well-balanced",           UserId = users[0].Id },
            new Coffee { Type = "Cortado",     Shop = "Ritual Coffee",  Location = "San Francisco, CA", Intensity = 7, Rating = 5, Temperature = "Hot",  Notes = "Perfect ratio, clean aftertaste",    UserId = users[1].Id },
            new Coffee { Type = "Flat White",  Shop = "Four Barrel",    Location = "San Francisco, CA", Intensity = 6, Rating = 3, Temperature = "Hot",  Notes = "A bit over-extracted today",         UserId = users[1].Id },
            new Coffee { Type = "Cold Brew",   Shop = "Philz Coffee",   Location = "Palo Alto, CA",     Intensity = 8, Rating = 4, Temperature = "Cold", Notes = "Strong and smooth, great for summer", UserId = users[2].Id },
            new Coffee { Type = "Iced Latte",  Shop = "Verve Coffee",   Location = "Santa Cruz, CA",    Intensity = 4, Rating = 4, Temperature = "Cold", Notes = "Light and refreshing",               UserId = users[2].Id },
        };

        db.Coffees.AddRange(coffees);
        await db.SaveChangesAsync();
    }
}
