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

        public async Task<string> CreateAccessTokenAsync(User user)
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

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<string> CreateRefreshTokenAsync()
        {
            var randomBytes = new byte[30];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return await Task.FromResult(Convert.ToBase64String(randomBytes));
        }

        public async Task<string> GetUsernameToAccesstokenAsync(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.AccessTokenKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException();

            var username = principal.FindFirst("userName")?.Value;

            if (username == null)
                throw new SecurityTokenException();

            return await Task.FromResult<string>(username);
        }
    }
}
