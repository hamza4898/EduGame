using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PurchaseCourse;

public sealed class PurchaseCourseCommandHandler(
    ICourseManagementService service,
    ICourseRepository courseRepository)
    : IRequestHandler<PurchaseCourseCommand, Result<StartCourseResponseDto>>
{
    public async Task<Result<StartCourseResponseDto>> Handle(PurchaseCourseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");
        if (request.Request?.CourseId <= 0) return Result.Invalid(new ValidationError("CourseId", "Идентификатор курса должен быть больше нуля."));

        var courseExists = await courseRepository.IsCourseExistsAsync(request.Request!.CourseId, cancellationToken);
        if (!courseExists) return Result.NotFound($"Курс с ID {request.Request.CourseId} не найден.");

        var result = await service.StartCourseAsync(request.UserId, request.Request, cancellationToken);
        return Result.Success(result);
    }
}
