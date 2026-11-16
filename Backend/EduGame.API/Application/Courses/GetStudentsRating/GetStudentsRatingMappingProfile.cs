using AutoMapper;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.GetStudentsRating;

public class GetStudentsRatingMappingProfile : Profile
{
    public GetStudentsRatingMappingProfile()
    {
        CreateMap<Student, StudentRatingResponseDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.ApplicationUser.UserName))
            .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.AvatarUrl))
            .ForMember(d => d.GlobalXp, o => o.MapFrom(s => s.GlobalXp))
            .ForMember(d => d.Level, o => o.MapFrom(s => 0));
    }
}