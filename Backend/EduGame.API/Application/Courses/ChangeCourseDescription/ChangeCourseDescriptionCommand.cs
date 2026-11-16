using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.ChangeCourseDescription;

public record ChangeCourseDescriptionCommand(
    string TeacherId,
    int CourseId,
    string CourseName,
    string CourseDescription,
    int SkillSubSectionId) : IRequest<Result>;