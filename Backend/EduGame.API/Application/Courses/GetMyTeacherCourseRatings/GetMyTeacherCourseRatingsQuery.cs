using Ardalis.Result;
using MediatR;

namespace EduGame.Application.Courses.GetMyTeacherCourseRatings;

public sealed record GetMyTeacherCourseRatingsQuery(string TeacherId)
    : IRequest<Result<List<TeacherCourseRatingDto>>>;
