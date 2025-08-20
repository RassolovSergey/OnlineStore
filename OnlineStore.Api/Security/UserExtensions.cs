using System.Security.Claims;

namespace OnlineStore.Api.Security
{
    public static class UserExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                throw new UnauthorizedAccessException("Некорректный идентификатор пользователя в токене.");

            return guid;
        }
    }
}
