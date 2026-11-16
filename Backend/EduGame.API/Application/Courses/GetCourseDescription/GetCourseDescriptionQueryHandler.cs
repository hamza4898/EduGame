using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCourseDescription;

public class GetCourseDescriptionQueryHandler(ICourseRepository courseRepository)
    : IRequestHandler<GetCourseDescriptionQuery, Result<GetCourseDescriptionResponseDto>>
{
    public async Task<Result<GetCourseDescriptionResponseDto>> Handle(GetCourseDescriptionQuery request, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetCourseForUpdateAsync(request.CourseId);

        if (course is null) return Result.NotFound("Курс не найден");

        var isOwner = course.Community.TeacherId == request.TeacherId;
        var hasAccess = await courseRepository.HasTeacherAccessAsync(request.CourseId, request.TeacherId, cancellationToken);
        
        if (!isOwner && !hasAccess) return Result.Forbidden("Данный курс не принадлежит вам");

        var courseDescription = await courseRepository.GetCourseDescription(request.CourseId);

        if (courseDescription is null) return Result.NotFound("Описание курса не найдено");

        return Result.Success(courseDescription);
    }
}