using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Cars.Queries;

public class GetFilteredCarsQuery : IRequest<PagedResult<CarDto>>
{
    public string? SearchTerm { get; set; }
    public string? Make { get; set; }
    public int? Year { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public int? SalesAgentId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}