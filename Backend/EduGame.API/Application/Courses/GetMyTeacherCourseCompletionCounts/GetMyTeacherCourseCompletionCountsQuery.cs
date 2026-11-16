using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourseCompletion;

public sealed record GetMyTeacherCourseCompletionCountsQuery(string TeacherId)
    : IRequest<Result<List<TeacherCourseCompletionCountDto>>>;