using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Ardalis.Result;
using EduGame.Entities;
using System.IO;
using System.Security.Cryptography;
using System.Security.Claims;

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

        public Result<string> GenerateToken(ApplicationUser user)
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
            var tokenString = tokenHandler.WriteToken(tokenDescriptor);

            return Result.Success(tokenString);
        }
    }
}