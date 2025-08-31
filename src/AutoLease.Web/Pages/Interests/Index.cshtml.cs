using AutoLease.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Web.Pages.Interests;

[Authorize(Roles = "SalesAgent")]
public class IndexModel : PageModel
{
    private readonly AutoLeaseDbContext _db;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AutoLeaseDbContext db, ILogger<IndexModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public record InterestListItem(
        int Id,
        int CarId,
        string CarDisplayName,
        string UserId,
        string ClientName,
        string Email,
        string Phone,
        DateTime PreferredCallTime,
        string? Notes,
        DateTime CreatedAt,
        List<string> DocumentLinks
    );

    public List<InterestListItem> Items { get; set; } = new();
    public string? Query { get; set; }
    public int Days { get; set; } = 7;
    public int TotalCount { get; set; }

    public async Task OnGetAsync(string? q, int? days)
    {
        Query = q;
        Days = days ?? 7;

        try
        {
            var fromDate = Days > 0 ? DateTime.UtcNow.AddDays(-Days) : (DateTime?)null;

            var queryable = _db.CarInterests
                .Include(ci => ci.Car)
                .Include(ci => ci.User)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                queryable = queryable.Where(ci => ci.CreatedAt >= fromDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var term = Query.Trim().ToLower();
                queryable = queryable.Where(ci =>
                    (ci.User.FirstName + " " + ci.User.LastName).ToLower().Contains(term) ||
                    (ci.User.Email ?? "").ToLower().Contains(term) ||
                    (ci.Car.Make + " " + ci.Car.Model).ToLower().Contains(term)
                );
            }

            TotalCount = await queryable.CountAsync();

            var results = await queryable
                .OrderByDescending(ci => ci.CreatedAt)
                .Take(500)
                .Select(ci => new
                {
                    ci.Id,
                    ci.CarId,
                    CarDisplayName = ci.Car.Year + " " + ci.Car.Make + " " + ci.Car.Model,
                    ci.UserId,
                    ClientName = ci.User.FirstName + " " + ci.User.LastName,
                    Email = ci.User.Email ?? string.Empty,
                    Phone = ci.User.PhoneNumber ?? string.Empty,
                    ci.PreferredCallTime,
                    ci.Notes,
                    ci.CreatedAt,
                    ci.DocumentPaths
                })
                .ToListAsync();

            Items = results.Select(r => new InterestListItem(
                r.Id,
                r.CarId,
                r.CarDisplayName,
                r.UserId,
                r.ClientName,
                r.Email,
                r.Phone,
                r.PreferredCallTime,
                r.Notes,
                r.CreatedAt,
                string.IsNullOrWhiteSpace(r.DocumentPaths)
                    ? new List<string>()
                    : r.DocumentPaths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load interests list");
            Items = new List<InterestListItem>();
            TotalCount = 0;
        }
    }
}