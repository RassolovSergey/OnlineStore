namespace OnlineStore.Domain.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public class User
    {
        // Поля
        public Guid Id { get; set; }    // Уникальный идентификатор пользователя
        public string Email { get; set; } = null!;  // Email пользователя
        public string NormalizedEmail { get; set; } = null!;    // Нормализованный email (для поиска, например, в БД)
        public string PasswordHash { get; set; } = null!;   // Пороль пользователя
        public DateTime CreatedAt { get; set; } // Дата регистрации пользователя
        public bool IsAdmin { get; set; }   // Флаг Администратора
        public bool IsDeleted { get; set; } // Мягкое удаление
        public DateTime? DeletedAtUtc { get; set; } // Дата удаления
        public Guid? DeletedBy { get; set; }    // Кем удалён (Id юзера/админа)

        // Связи
        public Cart Cart { get; set; } = null!; // 1:1 связь с сущностью Cart
        public ICollection<Order>? Orders { get; set; } // 1:N связь с сущностью Order
    }
}