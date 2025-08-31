using AutoLease.Application.DTOs;
using AutoLease.Application.Features.Cars.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.Cars.Handlers;

public class GetCarByIdQueryHandler : IRequestHandler<GetCarByIdQuery, CarDto?>
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;

    public GetCarByIdQueryHandler(ICarRepository carRepository, IMapper mapper)
    {
        _carRepository = carRepository;
        _mapper = mapper;
    }

    public async Task<CarDto?> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await _carRepository.GetByIdAsync(request.Id);
        
        return car == null ? null : _mapper.Map<CarDto>(car);
    }
}