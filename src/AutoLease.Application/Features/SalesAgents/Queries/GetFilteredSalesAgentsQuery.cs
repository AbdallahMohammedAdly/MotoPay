using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Queries;

public class GetFilteredSalesAgentsQuery : IRequest<PagedResult<SalesAgentDto>>
{
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public decimal? MinCommissionRate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}