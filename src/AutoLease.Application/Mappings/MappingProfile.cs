using AutoLease.Application.DTOs;
using AutoLease.Domain.Entities;
using AutoMapper;

namespace AutoLease.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Car, CarDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.GetFullName() : null))
            .ForMember(dest => dest.SalesAgentName, opt => opt.MapFrom(src => src.SalesAgent != null ? src.SalesAgent.GetFullName() : null));

        CreateMap<SalesAgent, SalesAgentDto>()
            .ForMember(dest => dest.AssignedCarsCount, opt => opt.MapFrom(src => src.AssignedCars.Count));

        CreateMap<User, UserDto>();

        CreateMap<Offer, OfferDto>();

        CreateMap<OfferApplication, OfferApplicationDto>();
    }
}