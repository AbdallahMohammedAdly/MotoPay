using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AutoLease.Web.Pages.Cars;

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

    public int CarId { get; set; }
    public List<SalesAgentDto> SalesAgents { get; set; } = new();
    public List<OfferDto> ActiveOffers { get; set; } = new();

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
        public string VinNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 1000000, ErrorMessage = "Price must be between $1 and $1,000,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? SalesAgentId { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        CarId = id;

        try
        {
            _logger.LogInformation("Loading car for editing: {CarId}", id);

            var query = new GetCarByIdQuery(id);
            var car = await _mediator.Send(query);

            if (car == null)
            {
                _logger.LogWarning("Car not found for editing: {CarId}", id);
                return NotFound();
            }

            // Map car data to input model
            Input = new InputModel
            {
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                Color = car.Color,
                VinNumber = car.VinNumber,
                Price = car.Price,
                Description = car.Description,
                ImageUrl = car.ImageUrl,
                SalesAgentId = car.SalesAgentId,
                IsAvailable = car.IsAvailable
            };

            await LoadSalesAgents();
            await LoadActiveOffers(id);

            _logger.LogInformation("Successfully loaded car for editing: {CarDisplayName}", car.DisplayName);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading car for editing: {CarId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        CarId = id;

        if (!ModelState.IsValid)
        {
            await LoadSalesAgents();
            await LoadActiveOffers(id);
            return Page();
        }

        try
        {
            _logger.LogInformation("Updating car: {CarId}", id);

            var command = new UpdateCarCommand
            {
                Id = id,
                Make = Input.Make,
                Model = Input.Model,
                Year = Input.Year,
                Color = Input.Color,
                Price = Input.Price,
                Description = Input.Description,
                ImageUrl = Input.ImageUrl,
                SalesAgentId = Input.SalesAgentId,
                IsAvailable = Input.IsAvailable
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully updated car: {CarDisplayName}", result.DisplayName);

            TempData["SuccessMessage"] = $"Car '{result.DisplayName}' has been updated successfully!";
            return RedirectToPage("./Details", new { id = id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update car due to business rule violation");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating car: {CarId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the car. Please try again.");
        }

        await LoadSalesAgents();
        await LoadActiveOffers(id);
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

    private async Task LoadActiveOffers(int carId)
    {
        try
        {
            var offersQuery = new GetOffersByCarIdQuery(carId);
            var allOffers = await _mediator.Send(offersQuery);
            ActiveOffers = allOffers.Where(o => o.IsActive).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load active offers for car ID: {CarId}", carId);
            ActiveOffers = new List<OfferDto>();
        }
    }
}