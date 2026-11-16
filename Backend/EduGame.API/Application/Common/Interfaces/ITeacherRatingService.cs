using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Common.Interfaces;

public interface ITeacherRatingService
{
    Task<List<TeacherRatingDto>> GetTopTeacherRatingsAsync(int count, CancellationToken cancellationToken);
}