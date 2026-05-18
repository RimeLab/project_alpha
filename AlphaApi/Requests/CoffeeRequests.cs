namespace AlphaApi.Requests;

public record CreateCoffeeRequest(string Type, string Shop, string Location, int Intensity, int Rating, string Temperature, string? Notes, int UserId);
public record UpdateCoffeeRequest(string Type, string Shop, string Location, int Intensity, int Rating, string Temperature, string? Notes, int UserId);
