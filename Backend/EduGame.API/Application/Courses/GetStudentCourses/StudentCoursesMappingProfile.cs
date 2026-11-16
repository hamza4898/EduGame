using AutoMapper;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.GetStudentCourses;

public class StudentCourseProfile : Profile
{
    public StudentCourseProfile()
    {
        CreateMap<StudentCourse, StudentCourseDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.Course.IsPaid))
            .ForMember(dest => dest.PriceCoins, opt => opt.MapFrom(src => src.Course.PriceCoins));
    }
}