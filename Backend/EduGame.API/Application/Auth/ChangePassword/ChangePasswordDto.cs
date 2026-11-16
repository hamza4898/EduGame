namespace EduGame.Application.Auth.ChangePassword;

public record ChangePasswordDto(Guid SessionId, string NewPassword);