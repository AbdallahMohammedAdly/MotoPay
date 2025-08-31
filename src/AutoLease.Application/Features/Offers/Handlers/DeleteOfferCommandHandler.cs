using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Domain.Interfaces;
using MediatR;

namespace AutoLease.Application.Features.Offers.Handlers;

public class DeleteOfferCommandHandler : IRequestHandler<DeleteOfferCommand>
{
    private readonly IOfferRepository _offerRepository;

    public DeleteOfferCommandHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdAsync(request.Id);
        
        if (offer == null)
            throw new KeyNotFoundException($"Offer with ID {request.Id} not found");

        await _offerRepository.DeleteAsync(request.Id);
    }
}