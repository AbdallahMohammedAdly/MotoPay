namespace AutoLease.Domain.Entities;

public class Offer
{
    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountedPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string Terms { get; private set; } = string.Empty;
    public int MaxApplications { get; private set; }
    public int CurrentApplications { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Foreign key
    public int CarId { get; private set; }
    public int? SalesAgentId { get; private set; }

    // Navigation properties
    public virtual Car Car { get; private set; } = null!;
    public virtual SalesAgent? SalesAgent { get; private set; }
    public virtual ICollection<OfferApplication> Applications { get; private set; } = new List<OfferApplication>();

    protected Offer() { } // For EF Core

    public Offer(string title, string description, decimal originalPrice, decimal discountedPrice,
                 DateTime startDate, DateTime endDate, string terms, int maxApplications, int carId, int? salesAgentId = null)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        ValidatePrices(originalPrice, discountedPrice);
        ValidateDates(startDate, endDate);
        ValidateTerms(terms);
        ValidateMaxApplications(maxApplications);

        Title = title;
        Description = description;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = CalculateDiscountPercentage(originalPrice, discountedPrice);
        StartDate = startDate;
        EndDate = endDate;
        Terms = terms;
        MaxApplications = maxApplications;
        CarId = carId;
        SalesAgentId = salesAgentId;
        IsActive = true;
        CurrentApplications = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateOffer(string title, string description, decimal originalPrice, decimal discountedPrice,
                           DateTime startDate, DateTime endDate, string terms, int maxApplications)
    {
        ValidateTitle(title);
        ValidateDescription(description);
        ValidatePrices(originalPrice, discountedPrice);
        ValidateDates(startDate, endDate);
        ValidateTerms(terms);
        ValidateMaxApplications(maxApplications);

        Title = title;
        Description = description;
        OriginalPrice = originalPrice;
        DiscountedPrice = discountedPrice;
        DiscountPercentage = CalculateDiscountPercentage(originalPrice, discountedPrice);
        StartDate = startDate;
        EndDate = endDate;
        Terms = terms;
        MaxApplications = maxApplications;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanApply()
    {
        return IsActive && 
               DateTime.UtcNow >= StartDate && 
               DateTime.UtcNow <= EndDate && 
               CurrentApplications < MaxApplications;
    }

    public void IncrementApplications()
    {
        if (!CanApply())
            throw new InvalidOperationException("Cannot apply to this offer");

        CurrentApplications++;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool HasStarted => DateTime.UtcNow >= StartDate;
    public int RemainingSlots => MaxApplications - CurrentApplications;
    public string DiscountLabel => $"{DiscountPercentage:F0}% OFF";

    private static decimal CalculateDiscountPercentage(decimal original, decimal discounted)
    {
        if (original <= 0) return 0;
        return ((original - discounted) / original) * 100;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters", nameof(title));
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
    }

    private static void ValidatePrices(decimal originalPrice, decimal discountedPrice)
    {
        if (originalPrice <= 0)
            throw new ArgumentException("Original price must be greater than zero", nameof(originalPrice));

        if (discountedPrice <= 0)
            throw new ArgumentException("Discounted price must be greater than zero", nameof(discountedPrice));

        if (discountedPrice >= originalPrice)
            throw new ArgumentException("Discounted price must be less than original price", nameof(discountedPrice));
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        if (startDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Start date cannot be in the past", nameof(startDate));
    }

    private static void ValidateTerms(string terms)
    {
        if (string.IsNullOrWhiteSpace(terms))
            throw new ArgumentException("Terms cannot be empty", nameof(terms));

        if (terms.Length > 2000)
            throw new ArgumentException("Terms cannot exceed 2000 characters", nameof(terms));
    }

    private static void ValidateMaxApplications(int maxApplications)
    {
        if (maxApplications <= 0)
            throw new ArgumentException("Max applications must be greater than zero", nameof(maxApplications));

        if (maxApplications > 1000)
            throw new ArgumentException("Max applications cannot exceed 1000", nameof(maxApplications));
    }
}