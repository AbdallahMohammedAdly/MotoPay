using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class GetFilteredSalesAgentsQueryHandler : IRequestHandler<GetFilteredSalesAgentsQuery, PagedResult<SalesAgentDto>>
{
    private readonly ISalesAgentRepository _salesAgentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFilteredSalesAgentsQueryHandler> _logger;

    public GetFilteredSalesAgentsQueryHandler(
        ISalesAgentRepository salesAgentRepository,
        IMapper mapper,
        ILogger<GetFilteredSalesAgentsQueryHandler> logger)
    {
        _salesAgentRepository = salesAgentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<SalesAgentDto>> Handle(GetFilteredSalesAgentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving filtered sales agents with search term: {SearchTerm}, Page: {PageNumber}", 
            request.SearchTerm, request.PageNumber);

        try
        {
            var salesAgents = await _salesAgentRepository.GetFilteredSalesAgentsAsync(
                request.SearchTerm,
                request.Department,
                request.MinCommissionRate,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var totalCount = await _salesAgentRepository.GetTotalCountAsync(
                request.SearchTerm,
                request.Department,
                request.MinCommissionRate);

            var salesAgentDtos = _mapper.Map<IEnumerable<SalesAgentDto>>(salesAgents);

            var result = new PagedResult<SalesAgentDto>(salesAgentDtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} sales agents out of {TotalCount} total", 
                salesAgentDtos.Count(), totalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving filtered sales agents");
            return new PagedResult<SalesAgentDto>();
        }
    }
}