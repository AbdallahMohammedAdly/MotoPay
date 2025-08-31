using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.Cars;

[Authorize(Roles = "SalesAgent")]
public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(IMediator mediator, ILogger<DeleteModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public CarDto? Car { get; set; }
    public int ActiveOffersCount { get; set; }
    public int ApplicationsCount { get; set; }
    public bool HasDependencies => ActiveOffersCount > 0 || ApplicationsCount > 0;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading car for deletion: {CarId}", id);

            var query = new GetCarByIdQuery(id);
            Car = await _mediator.Send(query);

            if (Car == null)
            {
                _logger.LogWarning("Car not found for deletion: {CarId}", id);
                return NotFound();
            }

            // Load dependency counts
            await LoadDependencyCounts(id);

            _logger.LogInformation("Successfully loaded car for deletion: {CarDisplayName}", Car.DisplayName);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading car for deletion: {CarId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete car: {CarId}", id);

            // Load car details for logging
            var query = new GetCarByIdQuery(id);
            var car = await _mediator.Send(query);

            if (car == null)
            {
                _logger.LogWarning("Car not found for deletion: {CarId}", id);
                return NotFound();
            }

            var command = new DeleteCarCommand(id);
            await _mediator.Send(command);

            _logger.LogInformation("Successfully deleted car: {CarDisplayName} (ID: {CarId})", car.DisplayName, id);

            TempData["SuccessMessage"] = $"Car '{car.DisplayName}' has been permanently deleted.";
            return RedirectToPage("./Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete car due to business rule violation: {CarId}", id);
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Details", new { id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting car: {CarId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the car. Please try again.";
            return RedirectToPage("./Details", new { id = id });
        }
    }

    private async Task LoadDependencyCounts(int carId)
    {
        try
        {
            var offersQuery = new GetOffersByCarIdQuery(carId);
            var offers = await _mediator.Send(offersQuery);
            ActiveOffersCount = offers.Count(o => o.IsActive);
            
            // For applications count, we'll need to count through offers
            ApplicationsCount = offers.Sum(o => o.CurrentApplications);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dependency counts for car ID: {CarId}", carId);
            ActiveOffersCount = 0;
            ApplicationsCount = 0;
        }
    }
}