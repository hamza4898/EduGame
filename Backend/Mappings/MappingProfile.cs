using AutoMapper;
using EduGame.Entities;
using EduGame.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudentDto, Student>().ReverseMap();
        CreateMap<TeacherDto, Teacher>().ReverseMap();
        CreateMap<PartnerDto, Partner>().ReverseMap();
    }
}