using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Offers.Commands;

public class UpdateOfferCommand : IRequest<OfferDto>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Terms { get; set; } = string.Empty;
    public int MaxApplications { get; set; }
    public int CarId { get; set; }
    public int? SalesAgentId { get; set; }
}