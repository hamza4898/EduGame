using AutoMapper;
using EduGame.Entities;
using EduGame.DTOs;
using System.Runtime.CompilerServices;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudentDTO, Student>();
        CreateMap<TeacherDTO, Teacher>();
        CreateMap<PartnerDTO, Partner>();
    }
}