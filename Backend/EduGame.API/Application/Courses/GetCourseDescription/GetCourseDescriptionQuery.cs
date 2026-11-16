using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetCourseDescription;

public record GetCourseDescriptionQuery(string TeacherId, int CourseId) : IRequest<Result<GetCourseDescriptionResponseDto>>;