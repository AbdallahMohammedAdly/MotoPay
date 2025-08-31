using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.Offers.Handlers;

public class UpdateOfferCommandHandler : IRequestHandler<UpdateOfferCommand, OfferDto>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IMapper _mapper;

    public UpdateOfferCommandHandler(IOfferRepository offerRepository, IMapper mapper)
    {
        _offerRepository = offerRepository;
        _mapper = mapper;
    }

    public async Task<OfferDto> Handle(UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdAsync(request.Id);
        
        if (offer == null)
            throw new KeyNotFoundException($"Offer with ID {request.Id} not found");

        offer.UpdateOffer(
            request.Title,
            request.Description,
            request.OriginalPrice,
            request.DiscountedPrice,
            request.StartDate,
            request.EndDate,
            request.Terms,
            request.MaxApplications
        );

        await _offerRepository.UpdateAsync(offer);

        return _mapper.Map<OfferDto>(offer);
    }
}