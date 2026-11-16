using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.RemoveCourseTeacher;

public sealed record RemoveCourseTeacherCommand(
    string UserId,
    int CourseId,
    string UserName) : IRequest<Result<bool>>;