namespace EduGame.Application.Common.Interfaces;

public interface ITeacherProgressionService
{
    Task AwardCoursePublishedAsync(int courseId, CancellationToken cancellationToken);

    Task AwardCourseCompletionAsync(int courseId, string studentId, CancellationToken cancellationToken);

    Task AwardCourseReviewAsync(int courseId, string studentId, CancellationToken cancellationToken);
}