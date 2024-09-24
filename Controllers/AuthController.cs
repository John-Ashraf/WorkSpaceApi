using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkSpaceApi.DTOS;
using WorkSpaceApi.Models;
using WorkSpaceApi.Services;

namespace WorkSpaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _AuthService;
        public AuthController(IAuth authServices)
        {
            _AuthService = authServices;

        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterationModel model)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            var AuthResult = await _AuthService.RegisterAsync(model);
            if (!AuthResult.IsAuthenticated)
            {
                return BadRequest(AuthResult.Message);
            }
            SetRefreshTokenInCookie(AuthResult.RefreshToken, AuthResult.RefreshTokenExpiration);
            return Ok(AuthResult);

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result=await _AuthService.LoginAsync(model);
            if(!result.IsAuthenticated)
            {
                return BadRequest(result.Message);
            }
            if(!string.IsNullOrEmpty(result.RefreshToken)) {
                result.Message += '1';
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            }
            return Ok(result);

        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result=await _AuthService.AddRole(model);
           
            return result.IsNullOrEmpty()? Ok(result): BadRequest(result);
            
        }
        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _AuthService.RefreshToken(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("RevokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeRequestModel model)
        {
            var token = model.token ?? Request.Cookies["refreshToken"];
            if (String.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }
            var result=await _AuthService.RevokeTokenAsync(token);
            if (!result)
            {
                return BadRequest("Invalid Token");
            }
            return Ok();

        }
        
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
