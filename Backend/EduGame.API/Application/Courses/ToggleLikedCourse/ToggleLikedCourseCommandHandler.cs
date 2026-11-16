using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.ToggleLikedCourse;

public class ToggleLikedCourseCommandHandler(ICourseRepository repository) : IRequestHandler<ToggleLikedCourseCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleLikedCourseCommand request, CancellationToken cancellationToken)
    {
        var courseLike = await repository.GetUserCourseLike(request.UserId, request.CourseId);

        if (courseLike != null)
        {
            repository.DeleteUserCourseLike(courseLike);
            await repository.Save();
            return Result.Success(false);
        }
        
        var newLike = new CourseLike
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            LikedAt = DateTime.UtcNow
        };

        repository.AddUserCourseLike(newLike);
        await repository.Save();

        return Result.Success(true);
    }
}