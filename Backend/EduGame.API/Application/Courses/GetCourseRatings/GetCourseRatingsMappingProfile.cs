using AutoMapper;
using EduGame.Application.Communities.GetCommunityRatings;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.GetCourseRatings;

public class GetCourseRatingsMappingProfile : Profile
{
    public GetCourseRatingsMappingProfile()
    {
        CreateMap<Course, CourseRawRatingDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Rating, o => o.MapFrom(s => 4.8))
            .ForMember(d => d.StudentsCount, o => o.MapFrom(s => s.Sections.SelectMany(sec => sec.Lessons).Count()))
            .ForMember(d => d.AvgProgress, o => o.MapFrom(s => 0.0));

        CreateMap<Community, CommunityRawRatingDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.CommunityName))
            .ForMember(d => d.MaxPossibleXp, o => o.MapFrom(s => 
            s.Courses.SelectMany(c => c.Sections).SelectMany(sec => sec.Lessons).Sum(l => l.XpReward)))
            .ForMember(d => d.TotalXp, o => o.MapFrom(s => 0));
    }
}