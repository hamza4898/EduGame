using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourseStudents;

public sealed record GetMyTeacherCourseStudentsQuery(string TeacherId)
    : IRequest<Result<List<TeacherCourseStudentDto>>>;