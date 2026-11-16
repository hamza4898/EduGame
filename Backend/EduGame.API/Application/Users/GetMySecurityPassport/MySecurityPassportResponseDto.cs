using EduGame.Domain.Enums;

namespace EduGame.Application.Users.GetMySecurityPassport;

public record MySecurityPassportResponseDto(
    string Id,
    DateTime RegisteredAt,
    UserRole UserRole,
    string Email,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool HasPassword,
    bool HasGoogleLogin,
    string? GoogleEmail
);