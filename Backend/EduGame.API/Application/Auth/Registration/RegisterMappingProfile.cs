using AutoMapper;
using EduGame.Domain.Entities;

namespace EduGame.Application.Auth.Registration;

public class RegisterMappingProfile : Profile
{
    public RegisterMappingProfile()
    {
        CreateMap<StudentDto, Student>().ReverseMap();

        CreateMap<TeacherDto, Teacher>().ReverseMap();

        CreateMap<BaseRegistrationDto, ApplicationUser>().ReverseMap();
    }
}