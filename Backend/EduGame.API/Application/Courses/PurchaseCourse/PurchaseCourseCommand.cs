using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.PurchaseCourse;

public sealed record PurchaseCourseCommand(string UserId, StartCourseRequestDto Request) : IRequest<Result<StartCourseResponseDto>>;