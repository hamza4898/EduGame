using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourseStudentCounts;

public sealed record GetMyTeacherCourseStudentCountsQuery(string TeacherId)
    : IRequest<Result<List<TeacherCourseStudentCountDto>>>;