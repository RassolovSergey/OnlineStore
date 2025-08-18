namespace OnlineStore.Domain.Entities
{
    public class Order
    {
        // Уникальный идентификатор заказа
        public Guid Id { get; set; }

        // Внешний ключ к пользователю, который сделал заказ
        public Guid UserId { get; set; }

        // Дата создания заказа
        public DateTime CreatedAt { get; set; }

        // Сумма заказа
        public decimal TotalAmount { get; set; }

        // Список 3D-моделей в заказе
        public User User { get; set; } = null!;

        // Список позиций заказа
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}