namespace AutoLease.Domain.Entities;

public class Car
{
    public int Id { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public string VinNumber { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Foreign keys
    public string? OwnerId { get; private set; }
    public int? SalesAgentId { get; private set; }

    // Navigation properties
    public virtual User? Owner { get; private set; }
    public virtual SalesAgent? SalesAgent { get; private set; }

    protected Car() { } // For EF Core

    public Car(string make, string model, int year, string color, string vinNumber, 
               decimal price, string description, string? imageUrl = null, int? salesAgentId = null)
    {
        ValidateMake(make);
        ValidateModel(model);
        ValidateYear(year);
        ValidateColor(color);
        ValidateVinNumber(vinNumber);
        ValidatePrice(price);
        ValidateDescription(description);

        Make = make;
        Model = model;
        Year = year;
        Color = color;
        VinNumber = vinNumber;
        Price = price;
        Description = description;
        ImageUrl = imageUrl;
        SalesAgentId = salesAgentId;
        IsAvailable = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string make, string model, int year, string color, 
                             decimal price, string description, string? imageUrl = null)
    {
        ValidateMake(make);
        ValidateModel(model);
        ValidateYear(year);
        ValidateColor(color);
        ValidatePrice(price);
        ValidateDescription(description);

        Make = make;
        Model = model;
        Year = year;
        Color = color;
        Price = price;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToOwner(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be empty", nameof(ownerId));

        OwnerId = ownerId;
        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakeAvailable()
    {
        OwnerId = null;
        IsAvailable = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSalesAgent(int salesAgentId)
    {
        if (salesAgentId <= 0)
            throw new ArgumentException("Invalid sales agent ID", nameof(salesAgentId));

        SalesAgentId = salesAgentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignSalesAgent()
    {
        SalesAgentId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAvailable()
    {
        IsAvailable = true;
        OwnerId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold(string? ownerId = null)
    {
        IsAvailable = false;
        if (!string.IsNullOrEmpty(ownerId))
        {
            OwnerId = ownerId;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetDisplayName() => $"{Year} {Make} {Model}";

    private static void ValidateMake(string make)
    {
        if (string.IsNullOrWhiteSpace(make))
            throw new ArgumentException("Make cannot be empty", nameof(make));

        if (make.Length > 50)
            throw new ArgumentException("Make cannot exceed 50 characters", nameof(make));
    }

    private static void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be empty", nameof(model));

        if (model.Length > 50)
            throw new ArgumentException("Model cannot exceed 50 characters", nameof(model));
    }

    private static void ValidateYear(int year)
    {
        var currentYear = DateTime.Now.Year;
        if (year < 1900 || year > currentYear + 1)
            throw new ArgumentException($"Year must be between 1900 and {currentYear + 1}", nameof(year));
    }

    private static void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentException("Color cannot be empty", nameof(color));

        if (color.Length > 30)
            throw new ArgumentException("Color cannot exceed 30 characters", nameof(color));
    }

    private static void ValidateVinNumber(string vinNumber)
    {
        if (string.IsNullOrWhiteSpace(vinNumber))
            throw new ArgumentException("VIN number cannot be empty", nameof(vinNumber));

        if (vinNumber.Length != 17)
            throw new ArgumentException("VIN number must be exactly 17 characters", nameof(vinNumber));
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));

        if (price > 1_000_000)
            throw new ArgumentException("Price cannot exceed $1,000,000", nameof(price));
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
    }
}