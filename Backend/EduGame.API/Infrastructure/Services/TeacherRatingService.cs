using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Services;

public sealed class TeacherRatingService(IUserRepository userRepository) : ITeacherRatingService
{
    public async Task<List<TeacherRatingDto>> GetTopTeacherRatingsAsync(int count, CancellationToken cancellationToken)
    {
        var rawData = await userRepository.GetRawTeachersAnalyticsAsync(cancellationToken);

        return rawData.Select(teacher =>
        {
            var averageProgress = teacher.Courses.Count == 0 ? 0 : teacher.Courses.Average(c => c.AverageProgress);
            var ratedCourses = teacher.Courses.Where(c => c.AverageRating > 0).ToList();
            var averageRating = ratedCourses.Count == 0 ? 0 : ratedCourses.Average(c => c.AverageRating);
            
            var score = averageRating * 1.5 + Math.Log2(teacher.UniqueStudentsCount + 1) + averageProgress * 0.05;

            return new TeacherRatingDto(
                teacher.Id,
                teacher.UserName,
                teacher.AvatarUrl,
                teacher.Courses.Count,
                teacher.UniqueStudentsCount,
                Math.Round(averageProgress, 1),
                Math.Round(averageRating, 1),
                Math.Round(score, 1));
        })
        .OrderByDescending(t => t.Score)
        .Take(count)
        .ToList();
    }
}