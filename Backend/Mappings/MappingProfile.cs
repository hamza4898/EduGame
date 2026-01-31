using AutoMapper;
using EduGame.Entities;
using EduGame.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudentDTO, Student>().ReverseMap();
        CreateMap<TeacherDTO, Teacher>().ReverseMap();
        CreateMap<PartnerDTO, Partner>().ReverseMap();
    }
}