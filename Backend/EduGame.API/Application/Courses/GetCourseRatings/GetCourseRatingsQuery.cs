using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.GetCourseRatings;

public record GetCourseRatingsQuery() : IRequest<Result<List<CourseRatingResponseDto>>>;