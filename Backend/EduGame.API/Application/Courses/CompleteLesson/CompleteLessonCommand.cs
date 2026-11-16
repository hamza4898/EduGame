using MediatR;
using Ardalis.Result;

namespace EduGame.Application.Courses.CompleteLesson;

public record CompleteLessonCommand(string UserId, int LessonId, int? SelectedAnswer) : IRequest<Result<LessonRewardDto>>;