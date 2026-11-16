using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.InviteCourseTeacher;

public sealed record InviteCourseTeacherCommand(
    string UserId,
    int CourseId,
    InviteCourseTeacherRequestDto Request) : IRequest<Result<bool>>;