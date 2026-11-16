using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using EduGame.Domain.Entities;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using EduGame.Application.Common.Interfaces;
using EduGame.Application.Auth.RefreshTokens;

namespace EduGame.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly ECDsaSecurityKey _key;
    private readonly IIdentityService _identityService;

    public JwtService(IIdentityService identityService)
    {
        _identityService = identityService;
        var ecdsa = ECDsa.Create();
        var privateKeyBase64 = Environment.GetEnvironmentVariable("EDUGAME_JWT_PRIVATE_KEY_BASE64");
        var privateKeyPem = string.IsNullOrWhiteSpace(privateKeyBase64)
            ? File.ReadAllText("private.key")
            : Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyBase64));

        ecdsa.ImportFromPem(privateKeyPem);

        _key = new ECDsaSecurityKey(ecdsa);
    }

    public async Task<string> GenerateAccessToken(ApplicationUser user)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.EcdsaSha256);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, user.UserRole.ToString()!)
        };

        var identityRoles = await _identityService.GetRolesAsync(user);
        claims.AddRange(identityRoles
            .Where(role => !string.Equals(role, user.UserRole.ToString(), StringComparison.OrdinalIgnoreCase))
            .Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new JwtSecurityToken(
            issuer: "EduGameServer",
            audience: "EduGameClient",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AuthTokenLifetime.AccessTokenMinutes),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(tokenDescriptor);

        return accessToken;
    }

    public RefreshTokenResponseDto GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        var refreshToken = Convert.ToBase64String(randomNumber);

        var hashedRefreshToken = HashToken(refreshToken);   

        return new RefreshTokenResponseDto(refreshToken, hashedRefreshToken);
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();

        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));

        var hashedRefreshToken = Convert.ToBase64String(hashedBytes); 

        return hashedRefreshToken;
    }
}