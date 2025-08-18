namespace OnlineStore.Domain.Entities

{
    /// <summary>
    /// Элемент корзины. Может ссылаться на модель или проект
    /// </summary>
    public class CartItem
    {
        // Уникальный идентификатор элемента корзины
        public Guid Id { get; set; }

        // Внешний ключ к корзине
        public Guid CartId { get; set; }

        // Внешний ключ к 3D модели (null - если это Проект)
        public Guid? Model3DId { get; set; }

        // Внешний ключ к проекту (null - если это 3D модель)
        public Guid? ProjectId { get; set; }

        // Дата добавления товара в корзину
        public DateTime AddedAt { get; set; }



        // Навигационные Свойства
        public Model3D? Model3D { get; set; }

        public Cart Cart { get; set; } = null!;

        public Project? Project { get; set; }
    }
}