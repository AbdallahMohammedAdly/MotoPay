using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Offers;

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

    public OfferDto? Offer { get; set; }
    public List<CarDto> AvailableCars { get; set; } = new();
    public List<SalesAgentDto> SalesAgents { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal OriginalPrice { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal DiscountedPrice { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 20)]
        public string Terms { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int MaxApplications { get; set; }

        [Required]
        public int CarId { get; set; }

        public int? SalesAgentId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var query = new GetOfferByIdQuery(id);
            Offer = await _mediator.Send(query);

            if (Offer == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = Offer.Id,
                Title = Offer.Title,
                Description = Offer.Description,
                OriginalPrice = Offer.OriginalPrice,
                DiscountedPrice = Offer.DiscountedPrice,
                StartDate = Offer.StartDate,
                EndDate = Offer.EndDate,
                Terms = Offer.Terms,
                MaxApplications = Offer.MaxApplications,
                CarId = Offer.CarId,
                SalesAgentId = Offer.SalesAgentId
            };

            await LoadData();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading offer for edit: {OfferId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.DiscountedPrice >= Input.OriginalPrice)
        {
            ModelState.AddModelError(nameof(Input.DiscountedPrice), "Discounted price must be less than original price");
        }

        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError(nameof(Input.EndDate), "End date must be after start date");
        }

        if (!ModelState.IsValid)
        {
            var query = new GetOfferByIdQuery(Input.Id);
            Offer = await _mediator.Send(query);
            await LoadData();
            return Page();
        }

        try
        {
            var command = new UpdateOfferCommand
            {
                Id = Input.Id,
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

            TempData["SuccessMessage"] = $"Offer '{result.Title}' has been updated successfully!";
            return RedirectToPage("./Details", new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating offer: {OfferId}", Input.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the offer.");
            
            var query = new GetOfferByIdQuery(Input.Id);
            Offer = await _mediator.Send(query);
            await LoadData();
            return Page();
        }
    }

    private async Task LoadData()
    {
        try
        {
            var carsQuery = new GetFilteredCarsQuery { IsAvailable = true, PageSize = 1000 };
            var carsResult = await _mediator.Send(carsQuery);
            AvailableCars = carsResult.Items.ToList();

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