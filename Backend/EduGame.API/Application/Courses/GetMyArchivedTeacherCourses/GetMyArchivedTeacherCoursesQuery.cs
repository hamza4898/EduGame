using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetMyArchivedTeacherCourses;

public sealed record GetMyArchivedTeacherCoursesQuery(string TeacherId)
    : IRequest<Result<List<MyTeacherCourseDto>>>;