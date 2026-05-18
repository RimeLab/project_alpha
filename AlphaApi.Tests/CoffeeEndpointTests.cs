namespace AlphaApi.Tests;

public class CoffeeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CoffeeEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CreateUserAsync(string username)
    {
        var response = await _client.PostAsJsonAsync("/users", new
        {
            username,
            password = "pass",
            description = (string?)null
        });
        var user = await response.Content.ReadFromJsonAsync<IdResponse>();
        return user!.Id;
    }

    private async Task<CoffeeResponse> CreateCoffeeAsync(int userId, int intensity = 5, int rating = 3)
    {
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso",
            shop = "Test Shop",
            location = "Test City",
            intensity,
            rating,
            temperature = "Hot",
            notes = (string?)null,
            userId
        });
        return (await response.Content.ReadFromJsonAsync<CoffeeResponse>())!;
    }

    [Fact]
    public async Task CreateCoffee_Returns201WithCoffeeData()
    {
        var userId = await CreateUserAsync("coffee_create_user");

        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Latte",
            shop = "Blue Bottle",
            location = "San Francisco",
            intensity = 5,
            rating = 4,
            temperature = "Hot",
            notes = "Smooth",
            userId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal("Latte", body.Type);
        Assert.Equal("Blue Bottle", body.Shop);
        Assert.Equal(5, body.Intensity);
        Assert.Equal(4, body.Rating);
        Assert.Equal(userId, body.UserId);
    }

    [Fact]
    public async Task CreateCoffee_Returns400_WhenIntensityTooLow()
    {
        var userId = await CreateUserAsync("coffee_intensity_low_user");
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 0, rating = 3, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCoffee_Returns400_WhenIntensityTooHigh()
    {
        var userId = await CreateUserAsync("coffee_intensity_high_user");
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 11, rating = 3, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCoffee_Returns400_WhenRatingTooLow()
    {
        var userId = await CreateUserAsync("coffee_rating_low_user");
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 5, rating = 0, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCoffee_Returns400_WhenRatingTooHigh()
    {
        var userId = await CreateUserAsync("coffee_rating_high_user");
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 5, rating = 6, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCoffee_Returns400_WhenUserNotFound()
    {
        var response = await _client.PostAsJsonAsync("/coffee", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 5, rating = 3, temperature = "Hot",
            notes = (string?)null, userId = 99999
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCoffees_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/coffee");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var coffees = await response.Content.ReadFromJsonAsync<List<CoffeeResponse>>();
        Assert.NotNull(coffees);
    }

    [Fact]
    public async Task GetCoffee_ReturnsCoffeeWithUser_WhenExists()
    {
        var userId = await CreateUserAsync("coffee_getbyid_user");
        var created = await CreateCoffeeAsync(userId);

        var response = await _client.GetAsync($"/coffee/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CoffeeDetailResponse>();
        Assert.Equal(created.Id, body!.Id);
        Assert.Equal(userId, body.UserId);
        Assert.NotNull(body.User);
        Assert.Equal(userId, body.User.Id);
    }

    [Fact]
    public async Task GetCoffee_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync("/coffee/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCoffee_ReturnsOkWithUpdatedData()
    {
        var userId = await CreateUserAsync("coffee_update_user");
        var created = await CreateCoffeeAsync(userId);

        var response = await _client.PutAsJsonAsync($"/coffee/{created.Id}", new
        {
            type = "Cold Brew",
            shop = "New Shop",
            location = "New City",
            intensity = 8,
            rating = 5,
            temperature = "Cold",
            notes = "Updated notes",
            userId
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        Assert.Equal("Cold Brew", body!.Type);
        Assert.Equal(8, body.Intensity);
        Assert.Equal(5, body.Rating);
    }

    [Fact]
    public async Task UpdateCoffee_Returns400_WhenIntensityInvalid()
    {
        var userId = await CreateUserAsync("coffee_update_intensity_user");
        var created = await CreateCoffeeAsync(userId);

        var response = await _client.PutAsJsonAsync($"/coffee/{created.Id}", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 0, rating = 3, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCoffee_Returns404_WhenNotFound()
    {
        var userId = await CreateUserAsync("coffee_update_404_user");
        var response = await _client.PutAsJsonAsync("/coffee/99999", new
        {
            type = "Espresso", shop = "S", location = "L",
            intensity = 5, rating = 3, temperature = "Hot",
            notes = (string?)null, userId
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCoffee_Returns204()
    {
        var userId = await CreateUserAsync("coffee_delete_user");
        var created = await CreateCoffeeAsync(userId);

        var response = await _client.DeleteAsync($"/coffee/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/coffee/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCoffee_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync("/coffee/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record IdResponse(int Id);
    private record CoffeeResponse(int Id, string Type, string Shop, string Location, int Intensity, int Rating, string Temperature, string? Notes, int UserId);
    private record CoffeeDetailResponse(int Id, string Type, string Shop, string Location, int Intensity, int Rating, string Temperature, string? Notes, int UserId, UserRef User);
    private record UserRef(int Id, string Username, string? Description);
}
