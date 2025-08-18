namespace OnlineStore.Domain.Entities

{
    /// <summary>
    /// Проект, объединяющий несколько моделей
    /// </summary>
    public class Project
    {
        // Уникальный идентификатор проекта
        public Guid Id { get; set; }

        // Название проекта
        public string Name { get; set; } = null!;

        // Описание проекта
        public string? Description { get; set; }

        // Цена проекта
        public decimal Price { get; set; }

        // Ссылка на внешний источник
        public string? CompanyUrl { get; set; }

        // Дата создания проекта
        public DateTime CreatedAt { get; set; }

        // Список 3D-моделей в проекте
        public ICollection<Model3D> Models { get; set; } = new List<Model3D>();

        // Галерея изображений проекта
        public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();

        // Список позиций заказа, связанных с проектом
        // (может быть null, если проект не был куплен)
        public ICollection<OrderItem>? OrderItems { get; set; }

        // Список позиций в корзине, связанных с проектом
        public ICollection<CartItem>? CartItems { get; set; }
    }
}