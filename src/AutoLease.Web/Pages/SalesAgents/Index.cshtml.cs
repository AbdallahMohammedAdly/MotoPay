using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.SalesAgents;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IMediator mediator, ILogger<IndexModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public PagedResult<SalesAgentDto> SalesAgents { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Department { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinCommissionRate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "CreatedAt";

    [BindProperty(SupportsGet = true)]
    public bool SortDescending { get; set; } = true;

    public List<string> AvailableDepartments { get; set; } = new();

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                   !string.IsNullOrEmpty(Department) || 
                                   MinCommissionRate.HasValue;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving filtered sales agents for display");
            
            var query = new GetFilteredSalesAgentsQuery
            {
                SearchTerm = SearchTerm,
                Department = Department,
                MinCommissionRate = MinCommissionRate,
                PageNumber = PageNumber,
                PageSize = 12,
                SortBy = SortBy,
                SortDescending = SortDescending
            };

            SalesAgents = await _mediator.Send(query);

            // Get available departments for filter dropdown
            await LoadAvailableDepartments();

            _logger.LogInformation("Successfully retrieved {Count} sales agents out of {TotalCount}", 
                SalesAgents.Items.Count(), SalesAgents.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving sales agents");
            SalesAgents = new PagedResult<SalesAgentDto>();
        }
    }

    public string GetPageUrl(int pageNumber)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (!string.IsNullOrEmpty(Department))
            queryParams.Add($"department={Uri.EscapeDataString(Department)}");
        if (MinCommissionRate.HasValue)
            queryParams.Add($"minCommissionRate={MinCommissionRate}");
        if (SortBy != "CreatedAt")
            queryParams.Add($"sortBy={SortBy}");
        if (!SortDescending)
            queryParams.Add($"sortDescending={SortDescending}");
        
        queryParams.Add($"pageNumber={pageNumber}");

        return $"/SalesAgents?{string.Join("&", queryParams)}";
    }

    public string GetSortUrl(string sortBy)
    {
        var sortDescending = SortBy == sortBy ? !SortDescending : true;
        
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (!string.IsNullOrEmpty(Department))
            queryParams.Add($"department={Uri.EscapeDataString(Department)}");
        if (MinCommissionRate.HasValue)
            queryParams.Add($"minCommissionRate={MinCommissionRate}");
        
        queryParams.Add($"sortBy={sortBy}");
        queryParams.Add($"sortDescending={sortDescending}");

        return $"/SalesAgents?{string.Join("&", queryParams)}";
    }

    public string GetSortDisplayName()
    {
        return SortBy switch
        {
            "FirstName" => SortDescending ? "First Name (Z-A)" : "First Name (A-Z)",
            "LastName" => SortDescending ? "Last Name (Z-A)" : "Last Name (A-Z)",
            "Department" => SortDescending ? "Department (Z-A)" : "Department (A-Z)",
            "CommissionRate" => SortDescending ? "Highest Commission" : "Lowest Commission",
            "AssignedCars" => SortDescending ? "Most Cars" : "Fewest Cars",
            _ => "Newest First"
        };
    }

    private async Task LoadAvailableDepartments()
    {
        try
        {
            var allAgentsQuery = new GetAllSalesAgentsQuery();
            var allAgents = await _mediator.Send(allAgentsQuery);
            AvailableDepartments = allAgents.Select(a => a.Department).Distinct().OrderBy(d => d).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load available departments");
            AvailableDepartments = new List<string> { "Sales", "Marketing", "Customer Service", "Management" };
        }
    }
}