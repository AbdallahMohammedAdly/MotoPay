using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;

namespace AutoLease.Web.Pages.Offers;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<DetailsModel> _logger;
    private readonly IOfferApplicationRepository _applicationRepository;
    private readonly IOfferRepository _offerRepository;

    public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger, IOfferApplicationRepository applicationRepository, IOfferRepository offerRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _applicationRepository = applicationRepository;
        _offerRepository = offerRepository;
    }

    public OfferDto? Offer { get; set; }
    public List<OfferApplicationDto> Applicants { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving offer details for ID: {OfferId}", id);

            var query = new GetOfferByIdQuery(id);
            Offer = await _mediator.Send(query);

            if (Offer == null)
            {
                _logger.LogWarning("Offer not found with ID: {OfferId}", id);
                return NotFound();
            }

            await LoadApplicantsAsync(id);

            _logger.LogInformation("Successfully retrieved offer details for: {OfferTitle}", Offer.Title);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving offer details for ID: {OfferId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostApplyAsync(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            // Prevent duplicate applications
            if (await _applicationRepository.ExistsAsync(id, userId))
            {
                TempData["SuccessMessage"] = "You have already applied to this offer.";
                return RedirectToPage(new { id });
            }

            var application = new OfferApplication(id, userId);
            await _applicationRepository.AddAsync(application);

            // Increment offer current applications
            var offer = await _offerRepository.GetByIdAsync(id);
            if (offer != null)
            {
                offer.IncrementApplications();
                await _offerRepository.UpdateAsync(offer);
            }

            TempData["SuccessMessage"] = "Application submitted successfully.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply for offer {OfferId}", id);
            TempData["ErrorMessage"] = "Failed to submit application.";
            return RedirectToPage(new { id });
        }
    }

    private async Task LoadApplicantsAsync(int offerId)
    {
        var apps = await _applicationRepository.GetByOfferIdAsync(offerId);
        Applicants = apps.Select(a => new OfferApplicationDto
        {
            Id = a.Id,
            ApplicationDate = a.ApplicationDate,
            Status = a.Status,
            Notes = a.Notes,
            ReviewedAt = a.ReviewedAt,
            ReviewNotes = a.ReviewNotes,
            OfferId = a.OfferId,
            UserId = a.UserId,
            ReviewedByUserId = a.ReviewedByUserId,
            User = new UserDto
            {
                Id = a.User.Id,
                Email = a.User.Email ?? string.Empty,
                FirstName = a.User.FirstName,
                LastName = a.User.LastName
            }
        }).ToList();
    }
}