using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Application.Courses.ChangeCourseDescription;

public class ChangeCourseDescriptionCommandHandler(
    ICourseRepository courseRepository,
    ISkillTreeRepository skillTreeRepository)
    : IRequestHandler<ChangeCourseDescriptionCommand, Result>
{
    public async Task<Result> Handle(ChangeCourseDescriptionCommand request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetCourseForUpdateAsync(request.CourseId);

        if (course is null) return Result.NotFound("Курс не найден.");

        var isOwner = course.Community.TeacherId == request.TeacherId;

        var hasAccess = await courseRepository.HasTeacherAccessAsync(request.CourseId, request.TeacherId, cancellationToken);
        
        if (!isOwner && !hasAccess) return Result.Forbidden("Вы не можете изменять описание курса, который не принадлежит вам.");

        var courseName = request.CourseName?.Trim();

        if (string.IsNullOrWhiteSpace(courseName) || courseName.Length is < 5 or > 200)
            return Result.Invalid(new ValidationError("Название курса должно содержать от 5 до 200 символов."));

        var courseDescription = request.CourseDescription?.Trim();

        if (string.IsNullOrWhiteSpace(courseDescription) || courseDescription.Length is < 10 or > 2000)
            return Result.Invalid(new ValidationError("Описание курса должно содержать от 10 до 2000 символов."));

        var skillSubSection = await skillTreeRepository.GetSkillSubSection(request.SkillSubSectionId);

        if (skillSubSection is null) return Result.Invalid(new ValidationError("Выбранная подкатегория обучения не найдена."));

        course.Title = courseName;
        course.Description = courseDescription;
        course.SkillSubSectionId = skillSubSection.Id;

        await courseRepository.Save();

        return Result.Success();
    }
}