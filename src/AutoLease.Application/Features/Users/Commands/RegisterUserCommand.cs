using AutoLease.Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Application.Features.Users.Commands;

public class RegisterUserCommand : IRequest<RegisterUserResult>
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}

public class RegisterUserResult
{
    public bool IsSuccess { get; set; }
    public string UserId { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public static RegisterUserResult Success(string userId)
    {
        return new RegisterUserResult
        {
            IsSuccess = true,
            UserId = userId
        };
    }

    public static RegisterUserResult Failure(IEnumerable<string> errors)
    {
        return new RegisterUserResult
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}