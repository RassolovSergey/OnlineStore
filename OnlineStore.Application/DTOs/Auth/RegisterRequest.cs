using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Application.DTOs.Auth;

/// <summary>
/// DTO для регистрации нового пользователя.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
