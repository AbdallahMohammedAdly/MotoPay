using Microsoft.AspNetCore.Identity;

namespace AutoLease.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public UserRole Role { get; private set; }

    // Navigation properties
    public virtual ICollection<Car> CarsOwned { get; private set; } = new List<Car>();
    public virtual SalesAgent? SalesAgent { get; private set; }

    protected User() { } // For EF Core

    public User(string email, string firstName, string lastName, UserRole role)
    {
        ValidateEmail(email);
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        Email = email;
        UserName = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email format", nameof(email));
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
}

public enum UserRole
{
    Client = 1,
    SalesAgent = 2
}