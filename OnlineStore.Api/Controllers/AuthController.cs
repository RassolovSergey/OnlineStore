using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineStore.Api.Extensions;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces.Services;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Аутентификация: регистрация, логин, профиль, смена пароля, refresh/logout.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Регистрация нового пользователя.
        /// Возвращает пару токенов (access + refresh).
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("ip-register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            // Валидацию делает FluentValidation; проверку уникальности — сервис
            var response = await _authService.RegisterAsync(request);
            return Ok(response); // мы сразу логиним, поэтому 200 OK (а не 201)
        }

        /// <summary>
        /// Логин по email/паролю.
        /// Возвращает пару токенов (access + refresh).
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("ip-login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Текущий профиль пользователя (по JWT).
        /// </summary>
        [HttpGet("me")]
        [Authorize] // требуется валидный access-токен
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userId = User.GetUserId();                 // из клейма sub/nameidentifier
            var profile = await _authService.GetProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Смена пароля текущего пользователя.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.GetUserId();
            await _authService.ChangePasswordAsync(userId, request);
            return NoContent();                            // 204 — по REST «успех без тела»
        }

        /// <summary>
        /// Обновление пары токенов по действующему refresh-токену (ротация).
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("ip-login")]                   // такой же лимит, как и у логина
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();
            var rsp = await _authService.RefreshAsync(request, ip, ua);
            return Ok(rsp);
        }

        /// <summary>
        /// Logout: отзыв конкретного refresh-токена.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _authService.LogoutAsync(request.RefreshToken, ip);
            return NoContent();
        }
    }
}