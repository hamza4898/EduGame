namespace EduGame.Application.Auth.DeleteAccount;

public record DeleteAccountDto(string UserId, string? Password, string? GoogleIdToken);