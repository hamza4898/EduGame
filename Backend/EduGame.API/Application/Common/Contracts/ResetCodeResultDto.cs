namespace EduGame.Application.Common.Contracts;

public record ResetCodeResultDto(Guid SessionId, string Code);