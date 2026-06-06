using CoparentHub.Application.Features;
using CoparentHub.Application.Features.Auth;
using CoparentHub.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoparentHub.Infrastructure
{
    public class JwtTokenService(IConfiguration config) : ITokenService
    {
        public string Generate(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("familyId", user.FamilyId?.ToString() ?? ""),
        };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw, 12);
        public bool Verify(string pw, string h) => BCrypt.Net.BCrypt.Verify(pw, h);
    }

}
