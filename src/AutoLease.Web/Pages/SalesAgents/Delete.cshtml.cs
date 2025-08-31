using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.SalesAgents;

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

    [BindProperty]
    public SalesAgentDto SalesAgent { get; set; } = new();

    public List<CarDto> RecentCars { get; set; } = new();
    public int AssignedCarsCount { get; set; }
    public int ActiveOffersCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading sales agent for deletion confirmation. ID: {SalesAgentId}", id);

            var query = new GetSalesAgentByIdQuery(id);
            var salesAgent = await _mediator.Send(query);

            if (salesAgent == null)
            {
                _logger.LogWarning("Sales agent not found with ID: {SalesAgentId}", id);
                return NotFound();
            }

            SalesAgent = salesAgent;

            // Load impact assessment data
            await LoadImpactAssessment(id);

            _logger.LogInformation("Successfully loaded sales agent for deletion: {FullName}", SalesAgent.FullName);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading sales agent for deletion. ID: {SalesAgentId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadImpactAssessment(SalesAgent.Id);
            return Page();
        }

        try
        {
            _logger.LogInformation("Deleting sales agent: {SalesAgentId}", SalesAgent.Id);

            var command = new DeleteSalesAgentCommand(SalesAgent.Id);
            await _mediator.Send(command);

            _logger.LogInformation("Successfully deleted sales agent: {FullName}", SalesAgent.FullName);

            TempData["SuccessMessage"] = $"Sales agent {SalesAgent.FullName} has been deleted successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting sales agent: {SalesAgentId}", SalesAgent.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the sales agent. Please try again.");
            
            await LoadImpactAssessment(SalesAgent.Id);
            return Page();
        }
    }

    private async Task LoadImpactAssessment(int salesAgentId)
    {
        try
        {
            // Load cars assigned to this sales agent
            var carsQuery = new GetFilteredCarsQuery
            {
                SalesAgentId = salesAgentId,
                PageNumber = 1,
                PageSize = 100 // Get all for counting
            };

            var carsResult = await _mediator.Send(carsQuery);
            RecentCars = carsResult.Items.ToList();
            AssignedCarsCount = carsResult.TotalCount;

            // TODO: Load active offers count when offers functionality is complete
            // For now, set to 0
            ActiveOffersCount = 0;

            _logger.LogInformation("Impact assessment loaded: {AssignedCars} cars, {ActiveOffers} offers", 
                AssignedCarsCount, ActiveOffersCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load impact assessment for sales agent ID: {SalesAgentId}", salesAgentId);
            RecentCars = new List<CarDto>();
            AssignedCarsCount = 0;
            ActiveOffersCount = 0;
        }
    }
}