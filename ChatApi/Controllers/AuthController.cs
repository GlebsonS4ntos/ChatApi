using ChatApi.Dto;
using ChatApi.Models;
using ChatApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _manager;
        private readonly JwtService _jwtService;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> manager, JwtService jwtService, SignInManager<User> signIn)
        {
            _manager = manager;
            _jwtService = jwtService;
            _signInManager = signIn;
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

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(login.UserEmail);

            if (user == null ) return Unauthorized();
            else if (await _manager.IsLockedOutAsync(user)) return Unauthorized("Tentou muito otario");

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, true);

            if (!result.Succeeded) return Unauthorized();

            await _manager.ResetAccessFailedCountAsync(user);

            var refreshToken = await _jwtService.CreateRefreshToken();
            user.RefreshToken = refreshToken;
            user.ValidRefreshToken = DateTime.UtcNow.AddDays(7);
            await _manager.UpdateAsync(user);

            var acessToken = await _jwtService.CreateAccessToken(user);

            return Ok(new
            {
                AcessToken = acessToken,
                RefreshToken = refreshToken
            });
        }
    }
}
