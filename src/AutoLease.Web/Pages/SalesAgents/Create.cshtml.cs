using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.SalesAgents;

[Authorize(Roles = "SalesAgent")]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IMediator mediator, ILogger<CreateModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<UserDto> AvailableUsers { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 50.0, ErrorMessage = "Commission rate must be between 0.1% and 50%")]
        [Display(Name = "Commission Rate")]
        public decimal CommissionRate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Biography")]
        public string Biography { get; set; } = string.Empty;

        [Display(Name = "User")]
        public string? UserId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAvailableUsers();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAvailableUsers();
            return Page();
        }

        try
        {
            _logger.LogInformation("Creating new sales agent: {FirstName} {LastName}", Input.FirstName, Input.LastName);

            var command = new CreateSalesAgentCommand
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Department = Input.Department,
                CommissionRate = Input.CommissionRate,
                Biography = Input.Biography,
                UserId = Input.UserId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully created sales agent with ID: {SalesAgentId}", result.Id);

            TempData["SuccessMessage"] = $"Sales agent {result.FullName} has been created successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating sales agent");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the sales agent. Please try again.");
            
            await LoadAvailableUsers();
            return Page();
        }
    }

    private async Task LoadAvailableUsers()
    {
        try
        {
            var query = new GetAllUsersQuery();
            AvailableUsers = (await _mediator.Send(query)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load available users");
            AvailableUsers = new List<UserDto>();
        }
    }
}