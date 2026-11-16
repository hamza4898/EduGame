using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetCourseReviews;

public sealed record GetCourseReviewsQuery(int CourseId) 
    : IRequest<Result<List<CourseReviewResponseDto>>>;