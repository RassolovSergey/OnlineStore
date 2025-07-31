using OnlineStore.Application.DTOs.Auth;

namespace OnlineStore.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса аутентификации и регистрации пользователей.
/// </summary>
public interface IAuthService
{
    // Регистрация нового пользователя
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    // Вход по email и паролю
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
