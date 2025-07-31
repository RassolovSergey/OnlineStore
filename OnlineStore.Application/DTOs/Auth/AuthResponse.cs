namespace OnlineStore.Application.DTOs.Auth;

/// <summary>
/// Ответ при успешной аутентификации.
/// </summary>
public class AuthResponse
{
    // Токен доступа
    public string Token { get; set; } = string.Empty;

    // Email пользователя
    public string Email { get; set; } = string.Empty;
}
