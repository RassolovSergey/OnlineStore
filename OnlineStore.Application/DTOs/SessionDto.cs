namespace OnlineStore.Application.DTOs.Auth
{
    /// <summary>
    /// Короткая проекция refresh-сессии для UI/Swagger.
    /// </summary>
    public class SessionDto
    {
        public Guid Id { get; set; }                 // Id refresh-токена (в БД)
        public DateTime CreatedAt { get; set; }      // Когда создан
        public DateTime ExpiresAt { get; set; }      // Когда истечёт
        public string? CreatedByIp { get; set; }     // IP при создании
        public string? CreatedByUa { get; set; }     // User-Agent при создании
        public bool IsActive { get; set; }           // Активен ли сейчас
        public bool IsCurrent { get; set; }          // Можно отметить текущую сессию (на шаге 4)
    }
}
