using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces;

public interface IProductRepository
{
    // CRUD операции для продуктов

    // Получить все продукты 
    Task<IEnumerable<Product>> GetAllAsync();
    // Получить продукт по ID
    Task<Product?> GetByIdAsync(Guid id);
    // Создать новый продукт
    Task AddAsync(Product product);
    // Обновить существующий продукт
    Task UpdateAsync(Product product);
    // Удалить продукт
    Task DeleteAsync(Product product);
}
