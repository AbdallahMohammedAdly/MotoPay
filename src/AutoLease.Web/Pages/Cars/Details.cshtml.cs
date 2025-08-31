using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Application.Features.Offers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AutoLease.Persistence.Data;
using AutoLease.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace AutoLease.Web.Pages.Cars;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DetailsModel> _logger;
    private readonly AutoLeaseDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger, AutoLeaseDbContext dbContext, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _dbContext = dbContext;
        _environment = environment;
    }

    public CarDto? Car { get; set; }
    public List<OfferDto> ActiveOffers { get; set; } = new();

    [BindProperty]
    public InterestInputModel Interest { get; set; } = new();

    public class InterestInputModel
    {
        [BindProperty]
        public DateTime? PreferredCallTime { get; set; }

        [BindProperty]
        public List<IFormFile> Documents { get; set; } = new();

        [BindProperty]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving car details for ID: {CarId}", id);

            var query = new GetCarByIdQuery(id);
            Car = await _mediator.Send(query);

            if (Car == null)
            {
                _logger.LogWarning("Car not found with ID: {CarId}", id);
                return NotFound();
            }

            // Load active offers for this car
            await LoadActiveOffers(id);

            _logger.LogInformation("Successfully retrieved car details for: {CarDisplayName}", Car.DisplayName);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving car details for ID: {CarId}", id);
            return RedirectToPage("./Index");
        }
    }

    private async Task LoadActiveOffers(int carId)
    {
        try
        {
            var offersQuery = new GetOffersByCarIdQuery(carId);
            var allOffers = await _mediator.Send(offersQuery);
            ActiveOffers = allOffers.Where(o => o.IsActive && o.CanApply).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load active offers for car ID: {CarId}", carId);
            ActiveOffers = new List<OfferDto>();
        }
    }

    public async Task<IActionResult> OnPostExpressInterestAsync(int carId)
    {
        try
        {
            // Basic validations
            if (Interest.PreferredCallTime == null)
            {
                ModelState.AddModelError("Interest.PreferredCallTime", "Preferred call time is required.");
            }
            else if (Interest.PreferredCallTime <= DateTime.Now)
            {
                ModelState.AddModelError("Interest.PreferredCallTime", "Preferred call time must be in the future.");
            }

            // Validate files
            var savedPaths = new List<string>();
            var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "application/pdf"
            };
            const long maxFileSizeBytes = 10 * 1024 * 1024; // 10MB

            if (Interest.Documents != null)
            {
                foreach (var file in Interest.Documents)
                {
                    if (file == null || file.Length == 0) continue;
                    if (!allowedContentTypes.Contains(file.ContentType))
                    {
                        ModelState.AddModelError("Interest.Documents", $"File '{file.FileName}' type is not allowed.");
                        continue;
                    }
                    if (file.Length > maxFileSizeBytes)
                    {
                        ModelState.AddModelError("Interest.Documents", $"File '{file.FileName}' exceeds 10MB limit.");
                        continue;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(carId);
                return Page();
            }

            // Save uploaded files under wwwroot/uploads/interests/{carId}/{unique}
            if (Interest.Documents != null && Interest.Documents.Count > 0)
            {
                var root = Path.Combine(_environment.WebRootPath, "uploads", "interests", carId.ToString());
                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                foreach (var file in Interest.Documents)
                {
                    if (file.Length == 0) continue;
                    var ext = Path.GetExtension(file.FileName);
                    var unique = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
                    var fullPath = Path.Combine(root, unique);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var relative = $"/uploads/interests/{carId}/{unique}";
                    savedPaths.Add(relative);
                }
            }

            // Persist interest
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var interest = new CarInterest(
                carId,
                userId,
                Interest.PreferredCallTime!.Value.ToUniversalTime(),
                Interest.Notes,
                savedPaths
            );

            _dbContext.CarInterests.Add(interest);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your interest has been submitted. We'll contact you at the selected time.";
            return RedirectToPage(new { id = carId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit interest for car {CarId}", carId);
            ModelState.AddModelError(string.Empty, "Failed to submit interest. Please try again.");
            await OnGetAsync(carId);
            return Page();
        }
    }
}