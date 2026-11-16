using AutoMapper;
using EduGame.Domain.Entities;

namespace EduGame.Application.Communities.CreateCommunity;

public class CommunityMappingProfile : Profile
{
    public CommunityMappingProfile()
    {
        CreateMap<CreateCommunityDto, Community>()
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.MembersCount, opt => opt.MapFrom(src => 1));
    }
}