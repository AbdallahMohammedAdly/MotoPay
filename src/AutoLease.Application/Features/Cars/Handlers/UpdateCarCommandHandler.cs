using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Cars.Handlers;

public class UpdateCarCommandHandler : IRequestHandler<UpdateCarCommand, CarDto>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCarCommandHandler> _logger;

    public UpdateCarCommandHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ILogger<UpdateCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CarDto> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating car with ID: {CarId}", request.Id);

        try
        {
            var car = await _carRepository.GetByIdAsync(request.Id);
            if (car == null)
            {
                throw new InvalidOperationException($"Car with ID {request.Id} not found");
            }

            // Update car details
            car.UpdateDetails(
                request.Make,
                request.Model,
                request.Year,
                request.Color,
                request.Price,
                request.Description,
                request.ImageUrl);

            // Update sales agent if provided
            if (request.SalesAgentId.HasValue)
            {
                car.AssignSalesAgent(request.SalesAgentId.Value);
            }
            else
            {
                car.UnassignSalesAgent();
            }

            // Update availability
            if (request.IsAvailable != car.IsAvailable)
            {
                if (request.IsAvailable)
                {
                    car.MarkAsAvailable();
                }
                else
                {
                    car.MarkAsSold();
                }
            }

            await _carRepository.UpdateAsync(car);

            var carDto = _mapper.Map<CarDto>(car);

            _logger.LogInformation("Successfully updated car: {CarDisplayName}", car.GetDisplayName());

            return carDto;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid car update data provided");
            throw new InvalidOperationException("Invalid car data: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating car with ID: {CarId}", request.Id);
            throw;
        }
    }
}