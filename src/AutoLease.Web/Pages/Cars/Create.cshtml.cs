using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Cars;

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

    public List<SalesAgentDto> SalesAgents { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Make is required")]
        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters")]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2030, ErrorMessage = "Year must be between 2000 and 2030")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "VIN Number is required")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN Number must be exactly 17 characters")]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$", ErrorMessage = "Invalid VIN format")]
        public string VinNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 1000000, ErrorMessage = "Price must be between $1 and $1,000,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? SalesAgentId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadSalesAgents();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSalesAgents();
            return Page();
        }

        try
        {
            _logger.LogInformation("Creating new car: {Make} {Model} {Year}", Input.Make, Input.Model, Input.Year);

            var command = new CreateCarCommand
            {
                Make = Input.Make,
                Model = Input.Model,
                Year = Input.Year,
                Color = Input.Color,
                VinNumber = Input.VinNumber.ToUpper(),
                Price = Input.Price,
                Description = Input.Description,
                ImageUrl = Input.ImageUrl,
                SalesAgentId = Input.SalesAgentId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully created car with ID: {CarId}", result.Id);

            TempData["SuccessMessage"] = $"Car '{result.DisplayName}' has been created successfully!";
            return RedirectToPage("./Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create car due to business rule violation");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating car");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the car. Please try again.");
        }

        await LoadSalesAgents();
        return Page();
    }

    private async Task LoadSalesAgents()
    {
        try
        {
            var query = new GetAllSalesAgentsQuery();
            SalesAgents = (await _mediator.Send(query)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load sales agents");
            SalesAgents = new List<SalesAgentDto>();
        }
    }
}