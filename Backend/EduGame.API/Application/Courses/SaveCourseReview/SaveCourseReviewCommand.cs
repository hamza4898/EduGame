using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.SaveCourseReview;

public sealed record SaveCourseReviewCommand(string UserId, int CourseId, SaveCourseReviewRequestDto Request) : IRequest<Result<CourseReviewResponseDto>>;