using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetCourseTeachers;

public sealed record GetCourseTeachersQuery(string UserId, int CourseId) 
    : IRequest<Result<List<CourseTeacherResponseDto>>>;