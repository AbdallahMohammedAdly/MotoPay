using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Offers.Handlers;

public class GetOfferByIdQueryHandler : IRequestHandler<GetOfferByIdQuery, OfferDto?>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOfferByIdQueryHandler> _logger;

    public GetOfferByIdQueryHandler(
        IOfferRepository offerRepository,
        IMapper mapper,
        ILogger<GetOfferByIdQueryHandler> logger)
    {
        _offerRepository = offerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OfferDto?> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving offer with ID: {OfferId}", request.Id);

        try
        {
            var offer = await _offerRepository.GetByIdAsync(request.Id);

            if (offer == null)
            {
                _logger.LogWarning("Offer not found with ID: {OfferId}", request.Id);
                return null;
            }

            var offerDto = _mapper.Map<OfferDto>(offer);

            _logger.LogInformation("Successfully retrieved offer: {OfferTitle}", offer.Title);

            return offerDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving offer with ID: {OfferId}", request.Id);
            return null;
        }
    }
}