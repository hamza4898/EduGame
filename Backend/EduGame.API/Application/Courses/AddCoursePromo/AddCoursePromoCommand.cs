using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.AddCoursePromo;

public sealed record AddCoursePromoCommand(string UserId, int CourseId, AddCoursePromoRequestDto Request) : IRequest<Result<CoursePromoResponseDto>>;