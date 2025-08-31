using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Cars.Queries;

public class GetCarByIdQuery : IRequest<CarDto?>
{
    public int Id { get; set; }

    public GetCarByIdQuery(int id)
    {
        Id = id;
    }
}