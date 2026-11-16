namespace EduGame.Application.Auth.VerifyCode;

public record VerifyCodeDto(Guid SessionId, string Code);