namespace EduGame.Application.Common.Contracts;

public sealed record CourseReadinessResponseDto(bool IsReady, IReadOnlyList<string> MissingRequirements);