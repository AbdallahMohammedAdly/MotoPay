using MediatR;

namespace AutoLease.Application.Features.Offers.Commands;

public class DeleteOfferCommand : IRequest
{
    public int Id { get; }

    public DeleteOfferCommand(int id)
    {
        Id = id;
    }
}