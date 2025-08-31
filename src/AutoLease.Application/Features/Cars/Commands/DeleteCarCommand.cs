using MediatR;

namespace AutoLease.Application.Features.Cars.Commands;

public class DeleteCarCommand : IRequest
{
    public int Id { get; set; }

    public DeleteCarCommand(int id)
    {
        Id = id;
    }
}