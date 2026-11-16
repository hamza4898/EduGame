namespace EduGame.Application.Auth.ChangeEmail;

public record ChangeEmailDto(string UserId, string CurrentPassword, string NewEmail);