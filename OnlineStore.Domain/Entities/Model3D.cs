namespace OnlineStore.Domain.Entities;

/// <summary>
/// 3D-модель, доступная для покупки или в составе проекта
/// </summary>
public class Model3D
{
    // Уникальный идентификатор модели
    public Guid Id { get; set; }

    // Название модели
    public string Name { get; set; } = null!;

    // Описание модели
    public string Description { get; set; } = null!;

    // Стоимость модели
    public decimal Price { get; set; }

    // Ссылка на внешний источник
    public string? CompanyUrl { get; set; }

    // Дата добавления модели
    public DateTime CreatedAt { get; set; }

    // Навинационное свойство
    public Guid? ProjectId { get; set; }

    // Навигационное свойство к проекту
    public Project? Project { get; set; }

    // Галерея изображений модели
    public ICollection<ModelImage> Images { get; set; } = new List<ModelImage>();
    // Список позиций заказа, связанных с моделью
    public ICollection<OrderItem>? OrderItems { get; set; }
    // Список позиций в корзине, связанных с моделью
    public ICollection<CartItem>? CartItems { get; set; }
}
