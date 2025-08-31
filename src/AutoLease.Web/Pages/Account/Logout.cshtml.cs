using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AutoLease.Domain.Entities;

namespace AutoLease.Web.Pages.Account;

[Authorize]
public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var userName = User.Identity?.Name ?? "Unknown";
            
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("User {UserName} logged out", userName);
            
            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            TempData["ErrorMessage"] = "An error occurred during logout. Please try again.";
            return Page();
        }
    }
}