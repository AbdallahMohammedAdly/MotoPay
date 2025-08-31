using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Commands;
using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Cars.Handlers;

public class CreateCarCommandHandler : IRequestHandler<CreateCarCommand, CarDto>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCarCommandHandler> _logger;

    public CreateCarCommandHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ILogger<CreateCarCommandHandler> logger)
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CarDto> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new car: {Make} {Model} {Year}", request.Make, request.Model, request.Year);

        // Check if VIN already exists
        if (await _carRepository.VinNumberExistsAsync(request.VinNumber))
        {
            _logger.LogWarning("Attempted to create car with existing VIN: {VinNumber}", request.VinNumber);
            throw new InvalidOperationException($"A car with VIN {request.VinNumber} already exists.");
        }

        var car = new Car(
            request.Make,
            request.Model,
            request.Year,
            request.Color,
            request.VinNumber,
            request.Price,
            request.Description,
            request.ImageUrl,
            request.SalesAgentId);

        var createdCar = await _carRepository.AddAsync(car);
        var carDto = _mapper.Map<CarDto>(createdCar);

        _logger.LogInformation("Successfully created car with ID: {CarId}", createdCar.Id);

        return carDto;
    }
}