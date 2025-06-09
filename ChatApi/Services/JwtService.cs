using ChatApi.Helpers;
using ChatApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatApi.Services
{
    public class JwtService
    {
        private readonly JwtConfig _jwtConfig;

        public JwtService(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig.Value;
        }

        public Task<string> CreateAccessToken(User user)
        {
            Claim[] userData = new[]
            {
                new Claim("userName", user.UserName!),
                new Claim("userId", user.Id)
            };

            var accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.AccessTokenKey));
            var signingCredentials = new SigningCredentials(accessKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: userData, 
                signingCredentials: signingCredentials, 
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.AcessTokenExpirationMinutes)
            );
            
            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public Task<string> CreateRefreshToken()
        {
            var randomBytes = new byte[30];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Task.FromResult(Convert.ToBase64String(randomBytes));
        }
    }
}
