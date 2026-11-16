using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.SaveCoursePrice;

public sealed record SaveCoursePriceCommand(string UserId, int CourseId, SaveCoursePriceRequestDto Request) : IRequest<Result<bool>>;