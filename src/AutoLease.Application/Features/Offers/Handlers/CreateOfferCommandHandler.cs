using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Offers.Commands;
using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Offers.Handlers;

public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, OfferDto>
{
    private readonly IOfferRepository _offerRepository;
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOfferCommandHandler> _logger;

    public CreateOfferCommandHandler(
        IOfferRepository offerRepository,
        ICarRepository carRepository,
        IMapper mapper,
        ILogger<CreateOfferCommandHandler> logger)
    {
        _offerRepository = offerRepository;
        _carRepository = carRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OfferDto> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new offer: {Title} for car ID: {CarId}", request.Title, request.CarId);

        try
        {
            // Validate that the car exists and is available
            var car = await _carRepository.GetByIdAsync(request.CarId);
            if (car == null)
            {
                throw new InvalidOperationException($"Car with ID {request.CarId} not found");
            }

            if (!car.IsAvailable)
            {
                throw new InvalidOperationException("Cannot create offer for a car that is not available");
            }

            // Create the offer entity
            var offer = new Offer(
                request.Title,
                request.Description,
                request.OriginalPrice,
                request.DiscountedPrice,
                request.StartDate,
                request.EndDate,
                request.Terms,
                request.MaxApplications,
                request.CarId,
                request.SalesAgentId);

            // Save to database
            var createdOffer = await _offerRepository.AddAsync(offer);

            // Map to DTO
            var offerDto = _mapper.Map<OfferDto>(createdOffer);

            _logger.LogInformation("Successfully created offer with ID: {OfferId}", createdOffer.Id);

            return offerDto;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid offer data provided");
            throw new InvalidOperationException("Invalid offer data: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating offer");
            throw;
        }
    }
}