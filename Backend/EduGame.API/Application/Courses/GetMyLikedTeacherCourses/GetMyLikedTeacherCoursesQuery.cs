using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetMyLikedTeacherCourses;

public sealed record GetMyLikedTeacherCoursesQuery(string TeacherId)
    : IRequest<Result<List<MyTeacherCourseDto>>>;