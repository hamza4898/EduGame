using AutoMapper;
using EduGame.Domain.Entities;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCatalogCourses;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Course, CatalogCoursesDto>()
            .ForMember(dest => dest.Owned, opt => opt.UseDestinationValue())
            .ForMember(dest => dest.Progress, opt => opt.UseDestinationValue())
            .ForMember(dest => dest.Community, opt => opt.MapFrom(src => src.Community));

        CreateMap<SkillSubSection, SkillSubSectionNestedDto>();

        CreateMap<SkillPath, SkillPathNestedDto>();

        CreateMap<Community, CommunityNestedDto>()
            .ForMember(dest => dest.CommunityId, opt => opt.MapFrom(src => src.CommunityId.ToString()));
    }
}