namespace OnlineStore.Domain.Entities
{
    /// <summary>
    /// Корзина пользователя
    /// </summary>
    public class Cart
    {
        // Уникальный идентификатор корзины
        public Guid Id { get; set; }

        // Ccвязь с пользователем
        // 1:1 связь с сущностью User
        public Guid UserId { get; set; }

        // Навигационное свойство к пользователю
        public User User { get; set; } = null!;

        // Коллекция элементов корзины
        // 1:N связь с сущностью CartItem

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}