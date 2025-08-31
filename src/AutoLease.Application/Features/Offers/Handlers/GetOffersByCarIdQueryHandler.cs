using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.Offers.Handlers;

public class GetOffersByCarIdQueryHandler : IRequestHandler<GetOffersByCarIdQuery, IEnumerable<OfferDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IMapper _mapper;

    public GetOffersByCarIdQueryHandler(IOfferRepository offerRepository, IMapper mapper)
    {
        _offerRepository = offerRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OfferDto>> Handle(GetOffersByCarIdQuery request, CancellationToken cancellationToken)
    {
        var offers = await _offerRepository.GetOffersByCarIdAsync(request.CarId);
        return _mapper.Map<IEnumerable<OfferDto>>(offers);
    }
}