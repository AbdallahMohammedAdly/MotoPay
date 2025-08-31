using AutoLease.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(UserManager<User> userManager, ILogger<IndexModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string UserRole { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    
    // Statistics properties
    public int TotalApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int CarsOwned { get; set; }
    public int CarsManaged { get; set; }
    public int OffersCreated { get; set; }
    public int TotalSales { get; set; }

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
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadUserData(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadUserData(user);
            return Page();
        }

        try
        {
            // Update user profile
            user.UpdateProfile(Input.FirstName, Input.LastName);
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadUserData(user);
                return Page();
            }

            _logger.LogInformation("User {UserId} updated their profile successfully", user.Id);
            
            TempData["StatusMessage"] = "Your profile has been updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating your profile. Please try again.");
            await LoadUserData(user);
            return Page();
        }
    }

    private async Task LoadUserData(User user)
    {
        Input.FirstName = user.FirstName;
        Input.LastName = user.LastName;
        Input.Email = user.Email ?? string.Empty;
        
        MemberSince = user.CreatedAt;
        
        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        UserRole = roles.FirstOrDefault() ?? "User";
        
        // Load statistics based on role
        await LoadUserStatistics(user);
    }

    private async Task LoadUserStatistics(User user)
    {
        try
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isClient = roles.Contains("Client");
            var isSalesAgent = roles.Contains("SalesAgent");

            if (isClient)
            {
                // For clients, we would typically load:
                // - Total applications submitted
                // - Approved applications
                // - Cars currently owned
                
                // Since we don't have these repositories injected, we'll set placeholder values
                // In a real implementation, you'd inject the necessary repositories
                TotalApplications = 0; // await _offerApplicationRepository.GetCountByUserIdAsync(user.Id);
                ApprovedApplications = 0; // await _offerApplicationRepository.GetApprovedCountByUserIdAsync(user.Id);
                CarsOwned = 0; // user.CarsOwned?.Count ?? 0;
            }
            else if (isSalesAgent)
            {
                // For sales agents, we would typically load:
                // - Cars managed
                // - Offers created
                // - Total sales
                
                CarsManaged = 0; // await _carRepository.GetCountBySalesAgentIdAsync(salesAgent.Id);
                OffersCreated = 0; // await _offerRepository.GetCountBySalesAgentIdAsync(salesAgent.Id);
                TotalSales = 0; // await _carRepository.GetSoldCountBySalesAgentIdAsync(salesAgent.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load user statistics for user {UserId}", user.Id);
            // Set default values on error
            TotalApplications = 0;
            ApprovedApplications = 0;
            CarsOwned = 0;
            CarsManaged = 0;
            OffersCreated = 0;
            TotalSales = 0;
        }
    }
}