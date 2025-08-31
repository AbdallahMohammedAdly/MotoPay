using AutoLease.Application.Features.SalesAgents.Commands;
using AutoLease.Domain.Interfaces;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Handlers;

public class DeleteSalesAgentCommandHandler : IRequestHandler<DeleteSalesAgentCommand>
{
    private readonly ISalesAgentRepository _salesAgentRepository;

    public DeleteSalesAgentCommandHandler(ISalesAgentRepository salesAgentRepository)
    {
        _salesAgentRepository = salesAgentRepository;
    }

    public async Task Handle(DeleteSalesAgentCommand request, CancellationToken cancellationToken)
    {
        var salesAgent = await _salesAgentRepository.GetByIdAsync(request.Id);
        
        if (salesAgent == null)
            throw new KeyNotFoundException($"Sales agent with ID {request.Id} not found");

        await _salesAgentRepository.DeleteAsync(request.Id);
    }
}