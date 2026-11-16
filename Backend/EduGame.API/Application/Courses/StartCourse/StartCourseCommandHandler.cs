using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.StartCourse;

public class StartCourseCommandHandler(ICourseManagementService courseManagement) : IRequestHandler<StartCourseCommand, Result>
{
    public async Task<Result> Handle(StartCourseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await courseManagement.StartCourseAsync(
                request.UserId,
                new StartCourseRequestDto(request.CourseId, null),
                cancellationToken);
            return Result.Success();
        }
        catch (KeyNotFoundException exception)
        {
            return Result.NotFound(exception.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Result.Forbidden();
        }
        catch (ArgumentException exception)
        {
            return Result.Invalid(new ValidationError(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Invalid(new ValidationError(exception.Message));
        }
    }
}