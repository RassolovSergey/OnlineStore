namespace OnlineStore.Infrastructure.Common
{
    // Класс для безопасного логирования данных
    // Используется для маскировки чувствительной информации, такой как email и токены
    public static class SafeLog
    {
        // Метод для безопасного логирования email
        // Маскирует email, оставляя только первую букву и домен
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var parts = email.Split('@');
            if (parts.Length != 2) return "***";
            var local = parts[0];
            var maskedLocal = local.Length <= 2 ? new string('*', local.Length)
                                                : local[0] + new string('*', Math.Max(1, local.Length - 2)) + local[^1];
            return $"{maskedLocal}@{parts[1]}";
        }

        // Метод для безопасного логирования токена
        // Маскирует токен, оставляя первые 6 и последние 4 символ
        public static string MaskToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return string.Empty;
            return token.Length <= 10 ? "****" : $"{token[..6]}...{token[^4..]}";
        }
    }
}