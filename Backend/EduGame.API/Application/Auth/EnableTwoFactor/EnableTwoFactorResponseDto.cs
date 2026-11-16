namespace EduGame.Application.Auth.EnableTwoFactor;

public record EnableTwoFactorResponseDto(string UnformattedKey, string Base64Image);