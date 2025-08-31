namespace AutoLease.Domain.Entities;

public class SalesAgent
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public decimal CommissionRate { get; private set; }
    public string Biography { get; private set; } = string.Empty;
    public DateTime HireDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Foreign key
    public string? UserId { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual ICollection<Car> AssignedCars { get; private set; } = new List<Car>();

    protected SalesAgent() { } // For EF Core

    public SalesAgent(string firstName, string lastName, string email, string phoneNumber,
                     string department, decimal commissionRate, string? userId = null, string biography = "")
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
        ValidateEmail(email);
        ValidatePhoneNumber(phoneNumber);
        ValidateDepartment(department);
        ValidateCommissionRate(commissionRate);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            ValidateUserId(userId);
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Department = department;
        CommissionRate = commissionRate;
        Biography = biography;
        UserId = userId;
        HireDate = DateTime.UtcNow;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, string email, 
                             string phoneNumber, string department)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
        ValidateEmail(email);
        ValidatePhoneNumber(phoneNumber);
        ValidateDepartment(department);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Department = department;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCommissionRate(decimal commissionRate)
    {
        ValidateCommissionRate(commissionRate);
        CommissionRate = commissionRate;
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

    public string GetFullName() => $"{FirstName} {LastName}";

    public decimal CalculateCommission(decimal saleAmount)
    {
        if (saleAmount <= 0)
            throw new ArgumentException("Sale amount must be positive", nameof(saleAmount));

        return saleAmount * (CommissionRate / 100);
    }

    public void UpdateDetails(string firstName, string lastName, string email, 
                             string phoneNumber, string department, decimal commissionRate, string biography)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
        ValidateEmail(email);
        ValidatePhoneNumber(phoneNumber);
        ValidateDepartment(department);
        ValidateCommissionRate(commissionRate);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Department = department;
        CommissionRate = commissionRate;
        Biography = biography ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
            
        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignUser()
    {
        UserId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{paramName} cannot be empty", paramName);

        if (name.Length < 2)
            throw new ArgumentException($"{paramName} must be at least 2 characters long", paramName);

        if (name.Length > 50)
            throw new ArgumentException($"{paramName} cannot exceed 50 characters", paramName);
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 100)
            throw new ArgumentException("Email cannot exceed 100 characters", nameof(email));
    }

    private static void ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        if (phoneNumber.Length < 10)
            throw new ArgumentException("Phone number must be at least 10 characters", nameof(phoneNumber));

        if (phoneNumber.Length > 15)
            throw new ArgumentException("Phone number cannot exceed 15 characters", nameof(phoneNumber));
    }

    private static void ValidateDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be empty", nameof(department));

        if (department.Length > 50)
            throw new ArgumentException("Department cannot exceed 50 characters", nameof(department));
    }

    private static void ValidateCommissionRate(decimal commissionRate)
    {
        if (commissionRate < 0)
            throw new ArgumentException("Commission rate cannot be negative", nameof(commissionRate));

        if (commissionRate > 50)
            throw new ArgumentException("Commission rate cannot exceed 50%", nameof(commissionRate));
    }

    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }
}