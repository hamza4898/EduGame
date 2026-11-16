using Ardalis.Result;
using EduGame.Application.Courses.CreateCourseWithCommunity;
using MediatR;

namespace EduGame.Application.Courses.GetCourseCreationOptions;

public record GetCourseCreationOptionsQuery : IRequest<Result<List<CourseCreationPathDto>>>;