using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class GetSalesAgentByIdQueryHandler : IRequestHandler<GetSalesAgentByIdQuery, SalesAgentDto?>
{
    private readonly ISalesAgentRepository _salesAgentRepository;
    private readonly IMapper _mapper;

    public GetSalesAgentByIdQueryHandler(ISalesAgentRepository salesAgentRepository, IMapper mapper)
    {
        _salesAgentRepository = salesAgentRepository;
        _mapper = mapper;
    }

    public async Task<SalesAgentDto?> Handle(GetSalesAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var salesAgent = await _salesAgentRepository.GetByIdAsync(request.Id);
        
        return salesAgent == null ? null : _mapper.Map<SalesAgentDto>(salesAgent);
    }
}