using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Domain.Entities;

namespace EduGame.Application.Courses.ToggleArchivedCourse;

public class ToggleArchivedCourseCommandHandler(ICourseRepository repository) : IRequestHandler<ToggleArchivedCourseCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleArchivedCourseCommand request, CancellationToken cancellationToken)
    {
        var archiveCourse = await repository.GetUserCourseArchive(request.UserId, request.CourseId);

        if (archiveCourse != null)
        {
            repository.DeleteUserCourseArchive(archiveCourse);
            await repository.Save();
            return Result.Success(false);
        }

        var newArchive = new CourseArchive
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            ArchivedAt = DateTime.UtcNow
        };

        repository.AddUserCourseArchive(newArchive);
        await repository.Save();

        return Result.Success(true);
    }
}