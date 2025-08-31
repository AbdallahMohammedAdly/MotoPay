using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Queries;

public class GetAllSalesAgentsQuery : IRequest<IEnumerable<SalesAgentDto>>
{
}