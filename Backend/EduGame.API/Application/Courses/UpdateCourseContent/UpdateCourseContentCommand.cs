using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.UpdateCourseContent;

public record UpdateCourseContentCommand(
    string TeacherId,
    int CourseId,
    UpdateCourseContentDto Content
) : IRequest<Result<GetCourseDescriptionResponseDto>>;