namespace EduGame.Application.Common.Contracts;

public sealed record NotificationResponseDto(int Id, string Sender, string Message, string Type, int? CourseId, Guid? TargetId, bool IsRead, DateTime CreatedAt);