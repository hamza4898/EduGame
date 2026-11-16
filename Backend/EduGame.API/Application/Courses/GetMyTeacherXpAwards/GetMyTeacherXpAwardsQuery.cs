using Ardalis.Result;
using MediatR;
using EduGame.Application.Common.Contracts;

namespace EduGame.Application.Courses.GetMyTeacherXpAwards;

public sealed record GetMyTeacherXpAwardsQuery(string TeacherId)
    : IRequest<Result<List<TeacherXpAwardDto>>>;