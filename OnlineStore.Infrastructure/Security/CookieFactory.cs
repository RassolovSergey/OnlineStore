using Microsoft.AspNetCore.Http;

namespace OnlineStore.Infrastructure.Security
{
    /// <summary>
    /// Фабрика настроек cookie для refresh-токена.
    /// </summary>
    public static class CookieFactory
    {
        // === Обёртки, которых не хватало контроллеру ===

        /// <summary>
        /// Опции для установки обычного refresh-cookie.
        /// Решение принимаем по текущему request (https?).
        /// </summary>
        public static CookieOptions Refresh(HttpRequest req) => new CookieOptions
        {
            HttpOnly = true,
            Secure = req.IsHttps,             // для https-профиля будет true
            SameSite = SameSiteMode.Lax,        // для Swagger (same-origin) этого достаточно
            Path = "/",                     // видимо на всё приложение (минимум — избежать чувствит. к регистру)
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };

        /// <summary>
        /// Опции для мгновенного удаления cookie (истёкшее).
        /// </summary>
        public static CookieOptions Expired(HttpRequest req)
        {
            var opts = Refresh(req);
            opts.Expires = DateTimeOffset.UtcNow.AddYears(-1);
            return opts;
        }

        // === Оставляем и «старые» методы — если где-то уже используются ===

        public static CookieOptions RefreshCookieOptions(bool isProd) => new CookieOptions
        {
            HttpOnly = true,
            Secure = isProd,
            // Если когда-то будешь отправлять refresh между разными origin (SPA),
            // переключишь на None, НО тогда Secure обязательно true.
            SameSite = isProd ? SameSiteMode.Lax : SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };

        public static CookieOptions RefreshCookieOptions(bool isProd, int refreshDays) => new CookieOptions
        {
            HttpOnly = true,
            Secure = isProd,
            SameSite = isProd ? SameSiteMode.Lax : SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(refreshDays)
        };

        public static CookieOptions ExpiredNow(bool isProd) => new CookieOptions
        {
            HttpOnly = true,
            Secure = isProd,
            SameSite = isProd ? SameSiteMode.Lax : SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddYears(-1)
        };
    }
}
