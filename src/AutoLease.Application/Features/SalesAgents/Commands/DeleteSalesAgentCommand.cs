using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Commands;

public class DeleteSalesAgentCommand : IRequest
{
    public int Id { get; }

    public DeleteSalesAgentCommand(int id)
    {
        Id = id;
    }
}