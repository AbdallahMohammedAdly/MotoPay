using AutoLease.Application.DTOs;
using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class UpdateSalesAgentCommandHandler : IRequestHandler<UpdateSalesAgentCommand, SalesAgentDto>
{
    private readonly ISalesAgentRepository _salesAgentRepository;
    private readonly IMapper _mapper;

    public UpdateSalesAgentCommandHandler(ISalesAgentRepository salesAgentRepository, IMapper mapper)
    {
        _salesAgentRepository = salesAgentRepository;
        _mapper = mapper;
    }

    public async Task<SalesAgentDto> Handle(UpdateSalesAgentCommand request, CancellationToken cancellationToken)
    {
        var salesAgent = await _salesAgentRepository.GetByIdAsync(request.Id);
        
        if (salesAgent == null)
            throw new KeyNotFoundException($"Sales agent with ID {request.Id} not found");

        salesAgent.UpdateDetails(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Department,
            request.CommissionRate,
            request.Biography
        );

        if (!string.IsNullOrEmpty(request.UserId))
        {
            salesAgent.AssignUser(request.UserId);
        }
        else
        {
            salesAgent.UnassignUser();
        }

        await _salesAgentRepository.UpdateAsync(salesAgent);

        return _mapper.Map<SalesAgentDto>(salesAgent);
    }
}