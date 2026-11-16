using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetStudentCourses;

public record GetStudentCoursesQuery(string StudentId) : IRequest<Result<List<CatalogCoursesDto>>>;