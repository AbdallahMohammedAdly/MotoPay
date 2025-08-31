using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Offers;

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

    public List<CarDto> AvailableCars { get; set; } = new();
    public List<SalesAgentDto> SalesAgents { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Title")]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Original Price")]
        [Range(0.01, 999999.99, ErrorMessage = "Original price must be greater than 0")]
        public decimal OriginalPrice { get; set; }

        [Required]
        [Display(Name = "Discounted Price")]
        [Range(0.01, 999999.99, ErrorMessage = "Discounted price must be greater than 0")]
        public decimal DiscountedPrice { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        [Required]
        [Display(Name = "Terms and Conditions")]
        [StringLength(2000, MinimumLength = 20)]
        public string Terms { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Maximum Applications")]
        [Range(1, 1000, ErrorMessage = "Maximum applications must be between 1 and 1000")]
        public int MaxApplications { get; set; } = 10;

        [Required]
        [Display(Name = "Car")]
        public int CarId { get; set; }

        [Display(Name = "Sales Agent")]
        public int? SalesAgentId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadData();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Custom validation
        if (Input.DiscountedPrice >= Input.OriginalPrice)
        {
            ModelState.AddModelError(nameof(Input.DiscountedPrice), "Discounted price must be less than original price");
        }

        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError(nameof(Input.EndDate), "End date must be after start date");
        }

        if (Input.StartDate <= DateTime.Now)
        {
            ModelState.AddModelError(nameof(Input.StartDate), "Start date must be in the future");
        }

        if (!ModelState.IsValid)
        {
            await LoadData();
            return Page();
        }

        try
        {
            _logger.LogInformation("Creating new offer: {Title}", Input.Title);

            var command = new CreateOfferCommand
            {
                Title = Input.Title,
                Description = Input.Description,
                OriginalPrice = Input.OriginalPrice,
                DiscountedPrice = Input.DiscountedPrice,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                Terms = Input.Terms,
                MaxApplications = Input.MaxApplications,
                CarId = Input.CarId,
                SalesAgentId = Input.SalesAgentId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully created offer with ID: {OfferId}", result.Id);

            TempData["SuccessMessage"] = $"Special offer '{result.Title}' has been created successfully!";
            return RedirectToPage("./Details", new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating offer");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the offer. Please try again.");
            
            await LoadData();
            return Page();
        }
    }

    private async Task LoadData()
    {
        try
        {
            // Load available cars
            var carsQuery = new GetFilteredCarsQuery
            {
                IsAvailable = true,
                PageSize = 1000
            };
            var carsResult = await _mediator.Send(carsQuery);
            AvailableCars = carsResult.Items.ToList();

            // Load sales agents
            var agentsQuery = new GetAllSalesAgentsQuery();
            SalesAgents = (await _mediator.Send(agentsQuery)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dropdown data");
            AvailableCars = new List<CarDto>();
            SalesAgents = new List<SalesAgentDto>();
        }
    }
}