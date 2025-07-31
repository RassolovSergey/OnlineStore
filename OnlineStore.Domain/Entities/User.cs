namespace OnlineStore.Domain.Entities;

/// <summary>
/// Сущность пользователя (упрощённая, без username).
/// </summary>
public class User
{
    public Guid Id { get; set; }

    // Уникальный email (используется как логин)
    public string Email { get; set; } = string.Empty;

    // Хешированный пароль
    public string PasswordHash { get; set; } = string.Empty;

    // Дата регистрации
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
