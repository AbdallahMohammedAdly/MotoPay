using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Queries;

public class GetSalesAgentByIdQuery : IRequest<SalesAgentDto?>
{
    public int Id { get; }

    public GetSalesAgentByIdQuery(int id)
    {
        Id = id;
    }
}