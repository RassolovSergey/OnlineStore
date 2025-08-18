namespace OnlineStore.Infrastructure.Options
{
    /// <summary>
    /// Настройки JWT, биндятся из секции "Jwt" в appsettings*.json.
    /// </summary>
    public class JwtOptions
    {
        /// <summary>Издатель токена (Issuer).</summary>
        public string Issuer { get; set; } = default!;

        /// <summary>Аудитория токена (Audience).</summary>
        public string Audience { get; set; } = default!;

        /// <summary>Секретный ключ для подписи (минимум 32 байта).</summary>
        public string Key { get; set; } = default!;

        /// <summary>Время жизни access-токена, в минутах.</summary>
        public int AccessTokenLifetimeMinutes { get; set; } = 120;

        public int RefreshTokenLifetimeDays { get; set; } = 30;

    }
}