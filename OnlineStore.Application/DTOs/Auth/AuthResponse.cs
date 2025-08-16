using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Application.DTOs.Auth;

/// <summary>
/// Ответ при успешной аутентификации.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
