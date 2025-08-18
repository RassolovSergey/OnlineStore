namespace OnlineStore.Application.DTOs.Auth
{
    /// <summary>
    /// Данные профиля текущего пользователя.
    /// </summary>
    public class UserProfileDto
    {
        // Идентификатор пользователя
        public Guid Id { get; set; }

        // Email пользователя
        public string Email { get; set; } = null!;

        // Дата создания профиля
        public DateTime CreatedAt { get; set; }
    }
}