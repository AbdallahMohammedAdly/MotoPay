using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Cars.Commands;

public class CreateCarCommand : IRequest<CarDto>
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public string VinNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? SalesAgentId { get; set; }
}