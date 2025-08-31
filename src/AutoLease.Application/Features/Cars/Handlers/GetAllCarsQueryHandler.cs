using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Cars.Handlers;

public class GetAllCarsQueryHandler : IRequestHandler<GetAllCarsQuery, IEnumerable<CarDto>>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCarsQueryHandler> _logger;

    public GetAllCarsQueryHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ILogger<GetAllCarsQueryHandler> logger)
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CarDto>> Handle(GetAllCarsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all cars");

        var cars = await _carRepository.GetAllAsync();
        var carDtos = _mapper.Map<IEnumerable<CarDto>>(cars);

        _logger.LogInformation("Retrieved {Count} cars", carDtos.Count());

        return carDtos;
    }
}