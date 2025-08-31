using AutoLease.Application.Features.Users.Commands;
using AutoLease.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(IMediator mediator, ILogger<RegisterModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account Type")]
        public string Role { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (ModelState.IsValid)
        {
            // Parse the role string to UserRole enum
            if (!Enum.TryParse<UserRole>(Input.Role, out var userRole))
            {
                ModelState.AddModelError(string.Empty, "Please select a valid account type.");
                return Page();
            }

            var command = new RegisterUserCommand
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                Password = Input.Password,
                Role = userRole
            };

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User created a new account with password.");
                
                // Redirect to login page with success message
                TempData["SuccessMessage"] = "Registration successful! Please log in with your new account.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}