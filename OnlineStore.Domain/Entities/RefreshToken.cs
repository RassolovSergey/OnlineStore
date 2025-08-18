namespace OnlineStore.Domain.Entities
{
    // Сущность, RefreshToken
    public class RefreshToken
    {
        public Guid Id { get; set; }    // Id Токена
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!;   // SHA-256 от «сырого» токена
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public string? CreatedByUa { get; set; }

        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public Guid? ReplacedByTokenId { get; set; }     // связь на новый токен при ротации
        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

        public User User { get; set; } = null!;
    }
}