using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.UpdateCourseContent;

public class UpdateCourseContentCommandHandler(
    ICourseRepository courseRepository,
    ICourseContentService contentService)
    : IRequestHandler<UpdateCourseContentCommand, Result<GetCourseDescriptionResponseDto>>
{
    public async Task<Result<GetCourseDescriptionResponseDto>> Handle(UpdateCourseContentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TeacherId)) return Result.Unauthorized("Не удалось определить пользователя.");

        var course = await courseRepository.GetCourseWithContentForUpdateAsync(request.CourseId);

        if (course is null) return Result.NotFound("Курс не найден.");

        var hasAccess = course.Community.TeacherId == request.TeacherId
            || await courseRepository.HasTeacherAccessAsync(request.CourseId, request.TeacherId, cancellationToken);

        if (!hasAccess) return Result.Forbidden("Вы не можете изменять содержание этого курса.");

        var sections = request.Content?.Sections;

        if (sections is null) return Result.Invalid(new ValidationError("Содержание курса не может быть пустым."));

        var validationError = contentService.ValidateContent(course, sections);

        if (validationError is not null) return Result.Invalid(new ValidationError(validationError));

        contentService.ApplyChanges(course, sections);

        await courseRepository.SaveChangesAsync(cancellationToken);

        var updatedCourse = await courseRepository.GetCourseDescription(request.CourseId);
        return updatedCourse is null
            ? Result.NotFound("Не удалось загрузить сохраненное содержание курса.")
            : Result.Success(updatedCourse);
    }
}