using AutoLease.Application.DTOs;
using MediatR;

namespace AutoLease.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
{
}