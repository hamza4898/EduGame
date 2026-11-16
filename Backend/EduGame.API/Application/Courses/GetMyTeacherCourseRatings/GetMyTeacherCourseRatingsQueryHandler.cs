using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.GetMyTeacherCourseRatings;

public class GetMyTeacherCourseRatingsQueryHandler(
    IUserRepository teacherRepository,
    ICourseRepository courseRepository)
    : IRequestHandler<GetMyTeacherCourseRatingsQuery, Result<List<TeacherCourseRatingDto>>>
{
    public async Task<Result<List<TeacherCourseRatingDto>>> Handle(GetMyTeacherCourseRatingsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var teacher = await teacherRepository.GetTeacherById(request.TeacherId);

        if (teacher is null) return Result.Forbidden("Рейтинги доступны только преподавателю.");

        var ratings = await courseRepository.GetTeacherCoursesRatingsAsync(request.TeacherId, cancellationToken);

        return Result.Success(ratings);
    }
}