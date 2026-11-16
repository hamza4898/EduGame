using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourseProgress;

public sealed record GetMyTeacherCourseProgressQuery(string TeacherId)
    : IRequest<Result<List<TeacherCourseProgressDto>>>;