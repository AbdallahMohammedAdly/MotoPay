using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.SalesAgents;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public SalesAgentDto? SalesAgent { get; set; }
    public List<CarDto> RecentCars { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving sales agent details for ID: {SalesAgentId}", id);

            var query = new GetSalesAgentByIdQuery(id);
            SalesAgent = await _mediator.Send(query);

            if (SalesAgent == null)
            {
                _logger.LogWarning("Sales agent not found with ID: {SalesAgentId}", id);
                return NotFound();
            }

            // Load recent cars managed by this sales agent
            await LoadRecentCars(id);

            _logger.LogInformation("Successfully retrieved sales agent details for: {FullName}", SalesAgent.FullName);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving sales agent details for ID: {SalesAgentId}", id);
            return RedirectToPage("./Index");
        }
    }

    private async Task LoadRecentCars(int salesAgentId)
    {
        try
        {
            // Load cars assigned to this sales agent
            var carsQuery = new GetFilteredCarsQuery
            {
                SalesAgentId = salesAgentId,
                PageNumber = 1,
                PageSize = 10,
                SortBy = "UpdatedAt",
                SortDescending = true
            };

            var carsResult = await _mediator.Send(carsQuery);
            RecentCars = carsResult.Items.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load recent cars for sales agent ID: {SalesAgentId}", salesAgentId);
            RecentCars = new List<CarDto>();
        }
    }
}