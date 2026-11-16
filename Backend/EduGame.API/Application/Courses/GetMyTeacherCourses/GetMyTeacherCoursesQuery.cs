using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourses;

public record GetMyTeacherCoursesQuery(string TeacherId) : IRequest<Result<List<MyTeacherCourseDto>>>;