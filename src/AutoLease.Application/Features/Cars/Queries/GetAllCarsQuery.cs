using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Cars.Queries;

public class GetAllCarsQuery : IRequest<IEnumerable<CarDto>>
{
}