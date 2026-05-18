namespace AlphaApi.Tests;

public class UserEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public UserEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ReturnsWelcomeMessage()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<WelcomeResponse>();
        Assert.Equal("Welcome", body!.Message);
    }

    [Fact]
    public async Task GetMetadata_ReturnsVersion()
    {
        var response = await _client.GetAsync("/metadata");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MetadataResponse>();
        Assert.NotEmpty(body!.Version);
    }

    [Fact]
    public async Task CreateUser_Returns201WithUserData()
    {
        var response = await _client.PostAsJsonAsync("/users", new
        {
            username = "create_user",
            password = "password123",
            description = "Test user"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal("create_user", body.Username);
        Assert.Equal("Test user", body.Description);
    }

    [Fact]
    public async Task CreateUser_PasswordIsStoredHashed()
    {
        const string password = "plaintext123";
        var createResponse = await _client.PostAsJsonAsync("/users", new
        {
            username = "hash_verify_user",
            password,
            description = (string?)null
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FindAsync(created!.Id);

        Assert.NotNull(user);
        Assert.NotEqual(password, user.Password);
        Assert.True(PasswordHasher.Verify(password, user.Password));
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        var createResponse = await _client.PostAsJsonAsync("/users", new
        {
            username = "getbyid_user",
            password = "pass",
            description = "desc"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();

        var response = await _client.GetAsync($"/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal(created.Id, body!.Id);
        Assert.Equal("getbyid_user", body.Username);
    }

    [Fact]
    public async Task GetUser_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync("/users/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOkWithUpdatedData()
    {
        var createResponse = await _client.PostAsJsonAsync("/users", new
        {
            username = "update_orig",
            password = "pass",
            description = "original"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();

        var response = await _client.PutAsJsonAsync($"/users/{created!.Id}", new
        {
            username = "update_new",
            password = "newpass",
            description = "updated"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("update_new", body!.Username);
        Assert.Equal("updated", body.Description);
    }

    [Fact]
    public async Task UpdateUser_Returns404_WhenNotFound()
    {
        var response = await _client.PutAsJsonAsync("/users/99999", new
        {
            username = "ghost",
            password = (string?)null,
            description = (string?)null
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Returns204()
    {
        var createResponse = await _client.PostAsJsonAsync("/users", new
        {
            username = "delete_user",
            password = "pass",
            description = (string?)null
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();

        var response = await _client.DeleteAsync($"/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync("/users/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record UserResponse(int Id, string Username, string? Description);
    private record WelcomeResponse(string Message);
    private record MetadataResponse(string Version);
}
