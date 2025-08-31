using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Offers.Queries;

public class GetOffersByCarIdQuery : IRequest<IEnumerable<OfferDto>>
{
    public int CarId { get; }

    public GetOffersByCarIdQuery(int carId)
    {
        CarId = carId;
    }
}