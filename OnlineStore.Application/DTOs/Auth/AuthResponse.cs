namespace OnlineStore.Application.DTOs.Auth
{
    /// <summary>
    /// Ответ при успешной аутентификации.
    /// </summary>
    public class AuthResponse
    {
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}