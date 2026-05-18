namespace AlphaApi.Entities;

public class Coffee
{
    public int Id { get; set; }
    public required string Type { get; set; }
    public required string Shop { get; set; }
    public required string Location { get; set; }
    public int Intensity { get; set; }
    public int Rating { get; set; }
    public required string Temperature { get; set; }
    public string? Notes { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
