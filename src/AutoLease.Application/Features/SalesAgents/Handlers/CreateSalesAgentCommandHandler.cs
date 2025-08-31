using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Domain.Entities;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class CreateSalesAgentCommandHandler : IRequestHandler<CreateSalesAgentCommand, SalesAgentDto>
{
    private readonly ISalesAgentRepository _salesAgentRepository;
    private readonly IMapper _mapper;

    public CreateSalesAgentCommandHandler(ISalesAgentRepository salesAgentRepository, IMapper mapper)
    {
        _salesAgentRepository = salesAgentRepository;
        _mapper = mapper;
    }

    public async Task<SalesAgentDto> Handle(CreateSalesAgentCommand request, CancellationToken cancellationToken)
    {
        var salesAgent = new SalesAgent(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Department,
            request.CommissionRate,
            request.UserId,
            request.Biography
        );

        await _salesAgentRepository.AddAsync(salesAgent);

        return _mapper.Map<SalesAgentDto>(salesAgent);
    }
}