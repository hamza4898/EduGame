namespace EduGame.Application.Auth.GoogleLogin;

public record GoogleLoginResponseDto(string AccessToken, string RefreshToken, bool IsProfileComplete);