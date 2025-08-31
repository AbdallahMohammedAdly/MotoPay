using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.SalesAgents.Commands;

public class CreateSalesAgentCommand : IRequest<SalesAgentDto>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public string Biography { get; set; } = string.Empty;
    public string? UserId { get; set; }
}