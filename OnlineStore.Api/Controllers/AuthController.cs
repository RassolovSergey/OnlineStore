using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineStore.Api.Extensions;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces.Services;

namespace OnlineStore.Api.Controllers;

/// <summary>
/// Контроллер для регистрации и логина пользователей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Сервис аутентификации, который будет использоваться для регистрации и логина
    private readonly IAuthService _authService;

    // Конструктор для внедрения зависимости сервиса аутентификации
    public AuthController(IAuthService authService)
    {
        // Инициализация сервиса аутентификации
        _authService = authService;
    }

    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("ip-register")] // Ограничение частоты запросов для регистрации
    [AllowAnonymous]    // Разрешаем доступ без аутентификации
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Вызов сервиса аутентификации для регистрации пользователя
        var response = await _authService.RegisterAsync(request);
        // Возвращаем ответ с данными пользователя и JWT-токеном
        // Если регистрация успешна, возвращаем статус 200 OK с данными пользователя
        return Ok(response);
    }

    /// <summary>
    /// Вход пользователя.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("ip-login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // Вызов сервиса аутентификации для входа пользователя

        var response = await _authService.LoginAsync(request);
        // Возвращаем ответ с данными пользователя и JWT-токеном
        // Если вход успешен, возвращаем статус 200 OK с данными пользователя
        return Ok(response);
    }

    /// <summary>
    /// Текущий профиль пользователя (по JWT).
    /// </summary>
    [HttpGet("me")]
    [Authorize] // требуется валидный токен
    [ProducesResponseType(typeof(OnlineStore.Application.DTOs.Auth.UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        // Достаём userId из клеймов
        var userId = User.GetUserId();

        // Берём профиль из сервиса
        var profile = await _authService.GetProfileAsync(userId);

        return Ok(profile);
    }

    /// <summary>Смена пароля текущего пользователя.</summary>
    [HttpPost("change-password")]
    [Authorize] // требуется валидный JWT
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        // 1) Достаём id из токена
        var userId = User.GetUserId();

        // 2) Делаем операцию в сервисе
        await _authService.ChangePasswordAsync(userId, request);

        // 3) Нечего отдавать — 204 No Content
        return NoContent();
    }
}
