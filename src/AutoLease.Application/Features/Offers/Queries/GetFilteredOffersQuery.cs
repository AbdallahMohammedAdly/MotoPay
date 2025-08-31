using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Offers.Queries;

public class GetFilteredOffersQuery : IRequest<PagedResult<OfferDto>>
{
    public string? SearchTerm { get; set; }
    public decimal? MinDiscount { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsExpired { get; set; }
    public int? CarId { get; set; }
    public int? SalesAgentId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}