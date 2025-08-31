using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Offers.Queries;

public class GetOfferByIdQuery : IRequest<OfferDto?>
{
    public int Id { get; set; }

    public GetOfferByIdQuery(int id)
    {
        Id = id;
    }
}