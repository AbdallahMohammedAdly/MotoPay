using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Cars.Commands;

public class UpdateCarCommand : IRequest<CarDto>
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? SalesAgentId { get; set; }
    public bool IsAvailable { get; set; } = true;
}