namespace EduGame.Application.Common.Contracts;

public record LoginResponseDto(string AccessToken, string RefreshToken, bool IsTwoFactorEnabled);