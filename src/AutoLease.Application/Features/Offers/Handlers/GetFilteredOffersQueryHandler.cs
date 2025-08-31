using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Offers.Handlers;

public class GetFilteredOffersQueryHandler : IRequestHandler<GetFilteredOffersQuery, PagedResult<OfferDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFilteredOffersQueryHandler> _logger;

    public GetFilteredOffersQueryHandler(
        IOfferRepository offerRepository,
        IMapper mapper,
        ILogger<GetFilteredOffersQueryHandler> logger)
    {
        _offerRepository = offerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<OfferDto>> Handle(GetFilteredOffersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving filtered offers with search term: {SearchTerm}, Page: {PageNumber}", 
            request.SearchTerm, request.PageNumber);

        try
        {
            var offers = await _offerRepository.GetFilteredOffersAsync(
                request.SearchTerm,
                request.MinDiscount,
                request.MaxPrice,
                request.IsActive,
                request.PageNumber,
                request.PageSize);

            var totalCount = await _offerRepository.GetTotalCountAsync(
                request.SearchTerm,
                request.MinDiscount,
                request.MaxPrice,
                request.IsActive);

            var offerDtos = _mapper.Map<IEnumerable<OfferDto>>(offers);

            var result = new PagedResult<OfferDto>(offerDtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} offers out of {TotalCount} total", 
                offerDtos.Count(), totalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving filtered offers");
            return new PagedResult<OfferDto>();
        }
    }
}