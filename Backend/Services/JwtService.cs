using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using EduGame.Entities;
using System.IO;
using System.Security.Cryptography;
using System.Security.Claims;
using EduGame.Data;
using EduGame.DTOs;
using System.Text;

namespace EduGame.Services
{
    public class JwtService : IJwtService
    {
        private readonly ECDsaSecurityKey _key;

        public JwtService()
        {
            var ecdsa = ECDsa.Create();

            ecdsa.ImportFromPem(File.ReadAllText("private.key"));

            _key = new ECDsaSecurityKey(ecdsa);
        }

        public string GenerateAccessToken(ApplicationUser user)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.EcdsaSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: "EduGameServer",
                audience: "EduGameClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
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
}