using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Stats.GetFocusTree;

public record GetFocusTreeQuery(string UserId, string PathCode) : IRequest<Result<List<FocusNodeDto>>>;