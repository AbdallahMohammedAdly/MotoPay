using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.Offers;

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

    public PagedResult<OfferDto> Offers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinDiscount { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "CreatedAt";

    [BindProperty(SupportsGet = true)]
    public bool SortDescending { get; set; } = true;

    public bool HasActiveFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                   MinDiscount.HasValue || 
                                   MaxPrice.HasValue || 
                                   IsActive.HasValue;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving filtered offers for display");
            
            var query = new GetFilteredOffersQuery
            {
                SearchTerm = SearchTerm,
                MinDiscount = MinDiscount,
                MaxPrice = MaxPrice,
                IsActive = IsActive,
                PageNumber = PageNumber,
                PageSize = 12,
                SortBy = SortBy,
                SortDescending = SortDescending
            };

            Offers = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved {Count} offers out of {TotalCount}", 
                Offers.Items.Count(), Offers.TotalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving offers");
            Offers = new PagedResult<OfferDto>();
        }
    }

    public string GetPageUrl(int pageNumber)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (MinDiscount.HasValue)
            queryParams.Add($"minDiscount={MinDiscount}");
        if (MaxPrice.HasValue)
            queryParams.Add($"maxPrice={MaxPrice}");
        if (IsActive.HasValue)
            queryParams.Add($"isActive={IsActive}");
        if (SortBy != "CreatedAt")
            queryParams.Add($"sortBy={SortBy}");
        if (!SortDescending)
            queryParams.Add($"sortDescending={SortDescending}");
        
        queryParams.Add($"pageNumber={pageNumber}");

        return $"/Offers?{string.Join("&", queryParams)}";
    }

    public string GetSortUrl(string sortBy)
    {
        var sortDescending = SortBy == sortBy ? !SortDescending : true;
        
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (MinDiscount.HasValue)
            queryParams.Add($"minDiscount={MinDiscount}");
        if (MaxPrice.HasValue)
            queryParams.Add($"maxPrice={MaxPrice}");
        if (IsActive.HasValue)
            queryParams.Add($"isActive={IsActive}");
        
        queryParams.Add($"sortBy={sortBy}");
        queryParams.Add($"sortDescending={sortDescending}");

        return $"/Offers?{string.Join("&", queryParams)}";
    }

    public string GetSortDisplayName()
    {
        return SortBy switch
        {
            "DiscountPercentage" => SortDescending ? "Highest Discount" : "Lowest Discount",
            "DiscountedPrice" => SortDescending ? "Highest Price" : "Lowest Price",
            "EndDate" => SortDescending ? "Latest Ending" : "Ending Soon",
            "CreatedAt" => SortDescending ? "Newest First" : "Oldest First",
            _ => "Newest First"
        };
    }
}