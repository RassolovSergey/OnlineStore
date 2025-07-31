namespace OnlineStore.Domain.Entities;

public class Product
{
    // Идентификатор продукта
    public Guid Id { get; set; }
    // Название продукта
    public string Name { get; set; } = string.Empty;
    // Цена продукта
    public decimal Price { get; set; }
    // Количество на складе
    public int Stock { get; set; }
}
