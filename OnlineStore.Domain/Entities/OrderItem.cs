namespace OnlineStore.Domain.Entities
{
    public class OrderItem
    {
        // Уникальный идентификатор элемента заказа
        public Guid Id { get; set; }

        // Внешний ключ к заказу
        public Guid OrderId { get; set; }

        // Внешний ключ к 3D-модели
        public Guid? Model3DId { get; set; }

        // Внешний ключ к проекту
        public Guid? ProjectId { get; set; }

        // Цену на момент покупки
        public decimal PriceAtPurchase { get; set; }

        // Связь с заказом, не может быть null
        public Order Order { get; set; } = null!;

        // Связь с 3D-моделью (null если выбран проект)
        public Model3D? Model3D { get; set; }

        // Связь с проектом (null если выбрана модель)
        public Project? Project { get; set; }
    }
}