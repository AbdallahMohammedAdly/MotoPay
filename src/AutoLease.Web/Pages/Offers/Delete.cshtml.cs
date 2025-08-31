using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Application.Features.Offers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLease.Web.Pages.Offers;

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
    public OfferDto Offer { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading offer for deletion confirmation. ID: {OfferId}", id);

            var query = new GetOfferByIdQuery(id);
            var offer = await _mediator.Send(query);

            if (offer == null)
            {
                _logger.LogWarning("Offer not found with ID: {OfferId}", id);
                return NotFound();
            }

            Offer = offer;

            _logger.LogInformation("Successfully loaded offer for deletion: {OfferTitle}", Offer.Title);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading offer for deletion. ID: {OfferId}", id);
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Deleting offer: {OfferId}", Offer.Id);

            var command = new DeleteOfferCommand(Offer.Id);
            await _mediator.Send(command);

            _logger.LogInformation("Successfully deleted offer: {OfferTitle}", Offer.Title);

            TempData["SuccessMessage"] = $"Offer '{Offer.Title}' has been deleted successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting offer: {OfferId}", Offer.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the offer. Please try again.");
            
            return Page();
        }
    }
}