using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.Cars;

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

    public PagedResult<CarDto> Cars { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Make { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsAvailable { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "CreatedAt";

    [BindProperty(SupportsGet = true)]
    public bool SortDescending { get; set; } = true;

    public List<string> AvailableMakes { get; set; } = new();

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                   !string.IsNullOrEmpty(Make) || 
                                   Year.HasValue || 
                                   MinPrice.HasValue || 
                                   MaxPrice.HasValue || 
                                   IsAvailable.HasValue;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving filtered cars for display");
            
            var query = new GetFilteredCarsQuery
            {
                SearchTerm = SearchTerm,
                Make = Make,
                Year = Year,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                IsAvailable = IsAvailable,
                PageNumber = PageNumber,
                PageSize = 12,
                SortBy = SortBy,
                SortDescending = SortDescending
            };

            Cars = await _mediator.Send(query);

            // Get available makes for filter dropdown
            await LoadAvailableMakes();

            _logger.LogInformation("Successfully retrieved {Count} cars out of {TotalCount}", 
                Cars.Items.Count(), Cars.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cars");
            Cars = new PagedResult<CarDto>();
        }
    }

    public string GetPageUrl(int pageNumber)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (!string.IsNullOrEmpty(Make))
            queryParams.Add($"make={Uri.EscapeDataString(Make)}");
        if (Year.HasValue)
            queryParams.Add($"year={Year}");
        if (MinPrice.HasValue)
            queryParams.Add($"minPrice={MinPrice}");
        if (MaxPrice.HasValue)
            queryParams.Add($"maxPrice={MaxPrice}");
        if (IsAvailable.HasValue)
            queryParams.Add($"isAvailable={IsAvailable}");
        if (SortBy != "CreatedAt")
            queryParams.Add($"sortBy={SortBy}");
        if (!SortDescending)
            queryParams.Add($"sortDescending={SortDescending}");
        
        queryParams.Add($"pageNumber={pageNumber}");

        return $"/Cars?{string.Join("&", queryParams)}";
    }

    public string GetSortUrl(string sortBy)
    {
        var sortDescending = SortBy == sortBy ? !SortDescending : true;
        
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (!string.IsNullOrEmpty(Make))
            queryParams.Add($"make={Uri.EscapeDataString(Make)}");
        if (Year.HasValue)
            queryParams.Add($"year={Year}");
        if (MinPrice.HasValue)
            queryParams.Add($"minPrice={MinPrice}");
        if (MaxPrice.HasValue)
            queryParams.Add($"maxPrice={MaxPrice}");
        if (IsAvailable.HasValue)
            queryParams.Add($"isAvailable={IsAvailable}");
        
        queryParams.Add($"sortBy={sortBy}");
        queryParams.Add($"sortDescending={sortDescending}");

        return $"/Cars?{string.Join("&", queryParams)}";
    }

    private async Task LoadAvailableMakes()
    {
        try
        {
            var allCarsQuery = new GetAllCarsQuery();
            var allCars = await _mediator.Send(allCarsQuery);
            AvailableMakes = allCars.Select(c => c.Make).Distinct().OrderBy(m => m).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load available makes");
            AvailableMakes = new List<string>();
        }
    }
}