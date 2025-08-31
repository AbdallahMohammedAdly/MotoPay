namespace AutoLease.Domain.Entities;

public class CarInterest
{
    public int Id { get; private set; }
    public int CarId { get; private set; }
    public string UserId { get; private set; } = string.Empty;

    public DateTime PreferredCallTime { get; private set; }
    public string? Notes { get; private set; }

    // Comma-separated relative paths to uploaded documents under wwwroot
    public string? DocumentPaths { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public virtual Car Car { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    protected CarInterest() { }

    public CarInterest(int carId, string userId, DateTime preferredCallTime, string? notes, IEnumerable<string>? documentPaths)
    {
        if (carId <= 0) throw new ArgumentException("Invalid car id", nameof(carId));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User id cannot be empty", nameof(userId));
        if (preferredCallTime <= DateTime.UtcNow.AddMinutes(-1)) throw new ArgumentException("Preferred call time must be in the future", nameof(preferredCallTime));
        if (notes != null && notes.Length > 500) throw new ArgumentException("Notes cannot exceed 500 characters", nameof(notes));

        CarId = carId;
        UserId = userId;
        PreferredCallTime = preferredCallTime;
        Notes = notes;
        DocumentPaths = documentPaths != null ? string.Join(',', documentPaths) : null;
        CreatedAt = DateTime.UtcNow;
    }
}