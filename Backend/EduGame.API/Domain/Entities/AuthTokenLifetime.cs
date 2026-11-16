namespace EduGame.Domain.Entities;

public static class AuthTokenLifetime
{
    public const int AccessTokenMinutes = 60;

    public const int RefreshTokenDays = 7;
}