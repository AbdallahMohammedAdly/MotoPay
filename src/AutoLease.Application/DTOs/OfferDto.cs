namespace AutoLease.Application.DTOs;

public class OfferDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string Terms { get; set; } = string.Empty;
    public int MaxApplications { get; set; }
    public int CurrentApplications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public int CarId { get; set; }
    public CarDto? Car { get; set; }
    public int? SalesAgentId { get; set; }
    public SalesAgentDto? SalesAgent { get; set; }
    
    // Computed properties
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool HasStarted => DateTime.UtcNow >= StartDate;
    public int RemainingSlots => MaxApplications - CurrentApplications;
    public string DiscountLabel => $"{DiscountPercentage:F0}% OFF";
    public bool CanApply => IsActive && HasStarted && !IsExpired && RemainingSlots > 0;
    public decimal Savings => OriginalPrice - DiscountedPrice;
    public string StatusBadge => GetStatusBadge();
    public string TimeRemaining => GetTimeRemaining();

    private string GetStatusBadge()
    {
        if (!IsActive) return "Inactive";
        if (IsExpired) return "Expired";
        if (!HasStarted) return "Coming Soon";
        if (RemainingSlots <= 0) return "Sold Out";
        return "Active";
    }

    private string GetTimeRemaining()
    {
        if (IsExpired) return "Expired";
        if (!HasStarted)
        {
            var timeToStart = StartDate - DateTime.UtcNow;
            if (timeToStart.Days > 0) return $"Starts in {timeToStart.Days} days";
            if (timeToStart.Hours > 0) return $"Starts in {timeToStart.Hours} hours";
            return "Starting soon";
        }

        var timeLeft = EndDate - DateTime.UtcNow;
        if (timeLeft.Days > 0) return $"{timeLeft.Days} days left";
        if (timeLeft.Hours > 0) return $"{timeLeft.Hours} hours left";
        if (timeLeft.Minutes > 0) return $"{timeLeft.Minutes} minutes left";
        return "Ending soon";
    }
}