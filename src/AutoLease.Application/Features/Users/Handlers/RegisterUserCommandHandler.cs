using AutoLease.Application.Features.Users.Commands;
using AutoLease.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Users.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return RegisterUserResult.Failure(new[] { "A user with this email already exists." });
            }

            // Create new user
            var user = new User(request.Email, request.FirstName, request.LastName, request.Role);
            
            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return RegisterUserResult.Failure(errors);
            }

            // Add user to appropriate role
            var roleName = request.Role.ToString();
            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to add user {UserId} to role {Role}", user.Id, roleName);
                // Don't fail registration if role assignment fails, just log it
            }

            _logger.LogInformation("User {UserId} registered successfully with role {Role}", user.Id, roleName);
            
            return RegisterUserResult.Success(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration for email {Email}", request.Email);
            return RegisterUserResult.Failure(new[] { "An error occurred during registration. Please try again." });
        }
    }
}