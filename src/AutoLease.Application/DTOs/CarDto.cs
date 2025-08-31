namespace AutoLease.Application.DTOs;

public class CarDto
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VinNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? OwnerId { get; set; }
    public int? SalesAgentId { get; set; }
    public string? OwnerName { get; set; }
    public string? SalesAgentName { get; set; }
    public string DisplayName => $"{Year} {Make} {Model}";

    public IReadOnlyList<string> Images
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ImageUrl)) return Array.Empty<string>();
            var separators = new[] { ',', '\n', ';', ' ' };
            return ImageUrl
                .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase) || u.StartsWith("/"))
                .Distinct()
                .ToList();
        }
    }
}