namespace AutoLease.Application.DTOs;

public class SalesAgentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UserId { get; set; }
    public string Biography { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public int AssignedCarsCount { get; set; }
    public decimal TotalSales { get; set; }
    public int CarsSold { get; set; }
}