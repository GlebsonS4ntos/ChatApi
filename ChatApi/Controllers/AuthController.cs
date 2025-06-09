using ChatApi.Dto;
using ChatApi.Models;
using ChatApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _manager;
        private readonly JwtService _jwtService;

        public AuthController(UserManager<User> manager, JwtService jwtService)
        {
            _manager = manager;
            _jwtService = jwtService;
        }

        [HttpPost("/createUser")]
        public async Task<IActionResult> CreateUser (CreateUserDto createUser)
        {
            var refreshToken = await _jwtService.CreateRefreshToken();

            var user = new User()
            {
                Email = createUser.UserEmail,
                UserName = createUser.UserName,
                RefreshToken = refreshToken,
                ValidRefreshToken = DateTime.UtcNow.AddDays(7)
            };

            var acessToken = await _jwtService.CreateAccessToken(user);

            IdentityResult resultCreateUser = await _manager.CreateAsync(user, createUser.Password);

            if (!resultCreateUser.Succeeded) return BadRequest(resultCreateUser.Errors);

            return Ok(new
            {
                AcessToken = acessToken,
                RefreshToken = refreshToken
            });
        }
    }
}
