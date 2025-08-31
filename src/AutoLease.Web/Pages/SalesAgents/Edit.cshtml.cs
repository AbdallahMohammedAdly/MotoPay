using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Application.Features.SalesAgents.Queries;
using AutoLease.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.SalesAgents;

[Authorize(Roles = "SalesAgent")]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IMediator mediator, ILogger<EditModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SalesAgentDto? SalesAgent { get; set; }
    public List<UserDto> AvailableUsers { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading sales agent for editing. ID: {SalesAgentId}", id);

            var query = new GetSalesAgentByIdQuery(id);
            SalesAgent = await _mediator.Send(query);

            if (SalesAgent == null)
            {
                _logger.LogWarning("Sales agent not found with ID: {SalesAgentId}", id);
                return NotFound();
            }

            // Populate input model
            Input = new InputModel
            {
                Id = SalesAgent.Id,
                FirstName = SalesAgent.FirstName,
                LastName = SalesAgent.LastName,
                Email = SalesAgent.Email,
                PhoneNumber = SalesAgent.PhoneNumber,
                Department = SalesAgent.Department,
                CommissionRate = SalesAgent.CommissionRate,
                Biography = SalesAgent.Biography,
                UserId = SalesAgent.UserId
            };

            await LoadAvailableUsers();

            _logger.LogInformation("Successfully loaded sales agent for editing: {FullName}", SalesAgent.FullName);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading sales agent for editing. ID: {SalesAgentId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload data for display
            var query = new GetSalesAgentByIdQuery(Input.Id);
            SalesAgent = await _mediator.Send(query);
            await LoadAvailableUsers();
            return Page();
        }

        try
        {
            _logger.LogInformation("Updating sales agent: {SalesAgentId}", Input.Id);

            var command = new UpdateSalesAgentCommand
            {
                Id = Input.Id,
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

            _logger.LogInformation("Successfully updated sales agent: {FullName}", result.FullName);

            TempData["SuccessMessage"] = $"Sales agent {result.FullName} has been updated successfully!";
            return RedirectToPage("./Details", new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating sales agent: {SalesAgentId}", Input.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the sales agent. Please try again.");
            
            // Reload data for display
            var query = new GetSalesAgentByIdQuery(Input.Id);
            SalesAgent = await _mediator.Send(query);
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