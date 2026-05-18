namespace AlphaApi.Requests;

public record CreateUserRequest(string Username, string Password, string? Description);
public record UpdateUserRequest(string Username, string? Password, string? Description);
