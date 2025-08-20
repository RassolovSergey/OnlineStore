using System.Text.Json.Serialization;

namespace OnlineStore.Application.DTOs.Auth
{
    /// <summary>
    /// Ответ аутентификации для клиента.
    /// Access-токен отдаём в теле, refresh-токен уходит только в HttpOnly cookie.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>Email пользователя.</summary>
        public string Email { get; set; } = default!;

        /// <summary>JWT access token.</summary>
        public string Token { get; set; } = default!;

        /// <summary>
        /// Сырой refresh-токен (используется контроллером, чтобы положить в cookie).
        /// В ответе JSON **не сериализуется**.
        /// </summary>
        [JsonIgnore] 
        public string? RefreshToken { get; set; }
    }
}
