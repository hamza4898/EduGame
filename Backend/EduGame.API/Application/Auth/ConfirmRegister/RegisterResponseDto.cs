namespace EduGame.Application.Auth.ConfirmRegister;

public record ConfirmEmailResponseDto(string ExternalId, string AccessToken, string RefreshToken);