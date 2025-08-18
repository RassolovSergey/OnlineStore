using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace OnlineStore.Api.Extensions
{
    /// <summary>
    /// Утилиты для чтения клеймов из токена.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Достаёт идентификатор пользователя из токена.
        /// Ищем сначала NameIdentifier, затем "sub".
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            // Проверяем, что пользователь авторизован
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)    // Ищем NameIdentifier
                      ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);  // Ищем "sub"

            // Если не нашли идентификатор, выбрасываем исключение
            if (!Guid.TryParse(raw, out var id))
                throw new UnauthorizedAccessException("Некорректный токен (нет корректного идентификатора пользователя).");

            // Возвращаем идентификатор пользователя
            return id;
        }
    }
}