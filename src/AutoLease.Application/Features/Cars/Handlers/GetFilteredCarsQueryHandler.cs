using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.Cars.Handlers;

public class GetFilteredCarsQueryHandler : IRequestHandler<GetFilteredCarsQuery, PagedResult<CarDto>>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFilteredCarsQueryHandler> _logger;

    public GetFilteredCarsQueryHandler(
        ICarRepository carRepository,
        IMapper mapper,
        ILogger<GetFilteredCarsQueryHandler> logger)
    {
        _carRepository = carRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<CarDto>> Handle(GetFilteredCarsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving filtered cars with search term: {SearchTerm}, Page: {PageNumber}", 
            request.SearchTerm, request.PageNumber);

        try
        {
            var cars = await _carRepository.GetFilteredCarsAsync(
                request.SearchTerm,
                request.Make,
                request.Year,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable,
                request.SalesAgentId,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var totalCount = await _carRepository.GetTotalCountAsync(
                request.SearchTerm,
                request.Make,
                request.Year,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable,
                request.SalesAgentId);

            var carDtos = _mapper.Map<IEnumerable<CarDto>>(cars);

            var result = new PagedResult<CarDto>(carDtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} cars out of {TotalCount} total", 
                carDtos.Count(), totalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving filtered cars");
            return new PagedResult<CarDto>();
        }
    }
}