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
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> manager, JwtService jwtService, SignInManager<User> signIn)
        {
            _manager = manager;
            _jwtService = jwtService;
            _signInManager = signIn;
        }

        [HttpPost("/createUser")]
        public async Task<IActionResult> CreateUser(CreateUserDto createUser)
        {
            var refreshToken = await _jwtService.CreateRefreshTokenAsync();

            var user = new User()
            {
                Email = createUser.UserEmail,
                UserName = createUser.UserName,
                RefreshToken = refreshToken,
                ValidRefreshToken = DateTime.UtcNow.AddDays(7)
            };

            var acessToken = await _jwtService.CreateAccessTokenAsync(user);

            IdentityResult resultCreateUser = await _manager.CreateAsync(user, createUser.Password);

            if (!resultCreateUser.Succeeded) return BadRequest(resultCreateUser.Errors);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new TokenDto
            {
                Accesstoken = acessToken
            });
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(login.UserEmail);

            if (user == null) return Unauthorized();
            else if (await _manager.IsLockedOutAsync(user)) return StatusCode(429);

            var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, true);

            if (!result.Succeeded) return Unauthorized();

            await _manager.ResetAccessFailedCountAsync(user);

            var refreshToken = await _jwtService.CreateRefreshTokenAsync();
            user.RefreshToken = refreshToken;
            user.ValidRefreshToken = DateTime.UtcNow.AddDays(7);
            await _manager.UpdateAsync(user);

            var acessToken = await _jwtService.CreateAccessTokenAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            return Ok(new TokenDto
            {
                Accesstoken = acessToken
            });
        }

        [HttpPost("/refresh")]
        public async Task<IActionResult> RefreshToken(TokenDto t)
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var username = await _jwtService.GetUsernameToAccesstokenAsync(t.Accesstoken);

                var user = await _signInManager.UserManager.FindByNameAsync(username);

                if (user == null || user.RefreshToken != refreshToken || user.ValidRefreshToken < DateTime.UtcNow) return Unauthorized();

                var rToken = await _jwtService.CreateRefreshTokenAsync();
                user.RefreshToken = rToken;
                user.ValidRefreshToken = DateTime.UtcNow.AddDays(7);
                await _manager.UpdateAsync(user);

                var acessToken = await _jwtService.CreateAccessTokenAsync(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                    Secure = true,
                    SameSite = SameSiteMode.None
                };

                Response.Cookies.Append("refreshToken", rToken, cookieOptions);

                return Ok(new TokenDto
                {
                    Accesstoken = acessToken
                });
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }
    }
}
