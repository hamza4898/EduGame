using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.GetStudentsRating;

public record GetStudentRatingsQuery() : IRequest<Result<List<StudentRatingResponseDto>>>;