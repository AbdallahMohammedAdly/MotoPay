using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Cars.Handlers;

public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand>
{
    private readonly ICarRepository _carRepository;
    private readonly ILogger<DeleteCarCommandHandler> _logger;

    public DeleteCarCommandHandler(
        ICarRepository carRepository,
        ILogger<DeleteCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _logger = logger;
    }

    public async Task Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting car with ID: {CarId}", request.Id);

        try
        {
            var car = await _carRepository.GetByIdAsync(request.Id);
            if (car == null)
            {
                throw new InvalidOperationException($"Car with ID {request.Id} not found");
            }

            await _carRepository.DeleteAsync(request.Id);

            _logger.LogInformation("Successfully deleted car: {CarDisplayName} (ID: {CarId})", 
                car.GetDisplayName(), request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting car with ID: {CarId}", request.Id);
            throw;
        }
    }
}