namespace OnlineStore.Domain.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public class User
    {
        // Уникальный идентификатор пользователя
        public Guid Id { get; set; }
        // Email пользователя
        public string Email { get; set; } = null!;

        // Нормализованный email (для поиска, например, в БД)
        public string NormalizedEmail { get; set; } = null!;

        // Пороль пользователя
        public string PasswordHash { get; set; } = null!;
        // Дата регистрации пользователя
        public DateTime CreatedAt { get; set; }

        // Навигационное свойство: корзина пользователя
        // 1:1 связь с сущностью Cart
        public Cart Cart { get; set; } = null!;

        // Навигационное свойство: список заказов пользователя
        // 1:N связь с сущностью Order
        public ICollection<Order>? Orders { get; set; }
    }
}