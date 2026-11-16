using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using MediatR;

namespace EduGame.Application.Courses.GetTeacherRatings;

public sealed record GetTeacherRatingsQuery : IRequest<Result<List<TeacherRatingDto>>>;