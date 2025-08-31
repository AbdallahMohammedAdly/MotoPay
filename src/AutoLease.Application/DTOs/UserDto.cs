using AutoLease.Domain.Entities;

namespace AutoLease.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string RoleName => Role.ToString();
}