using AutoMapper;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.GetCourse;

public class CourseDetailMappingProfile : Profile
{
    public CourseDetailMappingProfile()
    {
        CreateMap<Course, CourseDetailDto>();
        CreateMap<Section, SectionNestedDto>();
        CreateMap<Lesson, LessonNestedDto>();
        
        CreateMap<LessonBlock, BlockNestedDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type));
    }
}