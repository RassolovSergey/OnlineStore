using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces;

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
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // Вызов сервиса аутентификации для входа пользователя

        var response = await _authService.LoginAsync(request);
        // Возвращаем ответ с данными пользователя и JWT-токеном
        // Если вход успешен, возвращаем статус 200 OK с данными пользователя
        return Ok(response);
    }
}
