using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Queries;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class GetAllSalesAgentsQueryHandler : IRequestHandler<GetAllSalesAgentsQuery, IEnumerable<SalesAgentDto>>
{
    private readonly ISalesAgentRepository _salesAgentRepository;
    private readonly IMapper _mapper;

    public GetAllSalesAgentsQueryHandler(ISalesAgentRepository salesAgentRepository, IMapper mapper)
    {
        _salesAgentRepository = salesAgentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SalesAgentDto>> Handle(GetAllSalesAgentsQuery request, CancellationToken cancellationToken)
    {
        var salesAgents = await _salesAgentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SalesAgentDto>>(salesAgents);
    }
}