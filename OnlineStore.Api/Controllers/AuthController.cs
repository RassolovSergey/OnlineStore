using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Api.Security;             // User.GetUserId()
using OnlineStore.Infrastructure.Security;  // CookieNames, CookieFactory

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Аутентификация: регистрация/логин, refresh по cookie, управление сессиями.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IHostEnvironment _env;

        public AuthController(IAuthService auth, IHostEnvironment env)
        {
            _auth = auth;
            _env = env;
        }

        // -------- helpers --------

        private (string? ip, string? ua) GetClientInfo()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();
            return (ip, ua);
        }

        private bool IsProd() => _env.IsProduction();

        // -------- endpoints --------

        /// <summary>Регистрация: access в body, refresh — в HttpOnly cookie.</summary>
        [HttpPost("register")]
        [EnableRateLimiting("ip-register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var resp = await _auth.RegisterAsync(request);

            if (!string.IsNullOrWhiteSpace(resp.RefreshToken))
            {
                Response.Cookies.Append(
                    CookieNames.RefreshToken,
                    resp.RefreshToken,
                    CookieFactory.Refresh(Request)
                );
                resp.RefreshToken = null; // в тело не отдаём
            }

            return Ok(new AuthResponse { Email = resp.Email, Token = resp.Token });
        }

        /// <summary>Логин: access в body, refresh — в HttpOnly cookie.</summary>
        [HttpPost("login")]
        [EnableRateLimiting("ip-login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var resp = await _auth.LoginAsync(request);

            if (!string.IsNullOrWhiteSpace(resp.RefreshToken))
            {
                Response.Cookies.Append(
                    CookieNames.RefreshToken,
                    resp.RefreshToken,
                    CookieFactory.Refresh(Request)
                );
                resp.RefreshToken = null;
            }

            return Ok(new AuthResponse { Email = resp.Email, Token = resp.Token });
        }

        /// <summary>
        /// Идемпотентный refresh без тела: читает refresh из cookie, возвращает новый access и ротацию refresh-cookie.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            var raw = Request.Cookies[CookieNames.RefreshToken];
            if (string.IsNullOrWhiteSpace(raw))
                return Unauthorized();

            var (ip, ua) = GetClientInfo();
            var resp = await _auth.RefreshAsync(new RefreshRequest { RefreshToken = raw }, ip, ua);

            if (!string.IsNullOrWhiteSpace(resp.RefreshToken))
            {
                Response.Cookies.Append(
                    CookieNames.RefreshToken,
                    resp.RefreshToken,
                    CookieFactory.Refresh(Request)
                );
                resp.RefreshToken = null;
            }

            return Ok(new AuthResponse { Email = resp.Email, Token = resp.Token });
        }

        /// <summary>Выход из текущей сессии: отзывает refresh и стирает cookie.</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var raw = Request.Cookies[CookieNames.RefreshToken];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var (ip, _) = GetClientInfo();
                await _auth.LogoutAsync(raw, ip);

                Response.Cookies.Append(
                    CookieNames.RefreshToken,
                    string.Empty,
                    CookieFactory.Expired(Request)
                );
            }
            return NoContent();
        }

        /// <summary>Закрыть все мои сессии.</summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var (ip, _) = GetClientInfo();
            await _auth.LogoutAllAsync(User.GetUserId(), ip);

            Response.Cookies.Append(
                CookieNames.RefreshToken,
                string.Empty,
                CookieFactory.ExpiredNow(IsProd())
            );

            return NoContent();
        }

        /// <summary>Список моих сессий.</summary>
        [HttpGet("sessions")]
        [Authorize]
        public async Task<ActionResult<List<SessionDto>>> Sessions()
        {
            var list = await _auth.GetSessionsAsync(User.GetUserId());
            return Ok(list);
        }

        /// <summary>Закрыть конкретную мою сессию по Id refresh-токена.</summary>
        [HttpDelete("sessions/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> CloseSession(Guid id)
        {
            var (ip, _) = GetClientInfo();
            await _auth.LogoutSessionAsync(User.GetUserId(), id, ip);
            return NoContent();
        }

        /// <summary>[Admin] Закрыть все сессии указанного пользователя.</summary>
        [HttpPost("admin/logout-all/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminLogoutAll(Guid userId)
        {
            var (ip, _) = GetClientInfo();
            await _auth.LogoutAllAsync(userId, ip);
            return NoContent();
        }
    }
}