using EduGame.Application.Communities.GetCommunityRatings;
using EduGame.Application.Courses.GetCourseRatings;
using EduGame.Application.Courses.GetStudentsRating;

namespace EduGame.Application.Common.Interfaces;

public interface IRatingRepository
{
    Task<List<CourseRawRatingDto>> GetRawCourseRatingsAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken);

    Task<List<CommunityRawRatingDto>> GetRawCommunityRatingsAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken);

    Task<List<StudentRatingResponseDto>> GetTopStudentsRawAsync(AutoMapper.IConfigurationProvider mapperConfig, CancellationToken cancellationToken);
}