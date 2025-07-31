using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Application.DTOs.Auth;

/// <summary>
/// DTO для входа по email и паролю.
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
