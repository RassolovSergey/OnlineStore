using OnlineStore.Application.DTOs;

namespace OnlineStore.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с продуктами.
/// Выполняет бизнес-логику и обрабатывает DTO.
/// </summary>
public interface IProductService
{
    // Получить список всех продуктов
    Task<IEnumerable<ProductDto>> GetAllAsync();

    // Получить продукт по идентификатору
    Task<ProductDto?> GetByIdAsync(Guid id);

    // Создать продукт
    Task<ProductDto> CreateAsync(ProductDto dto);

    // Обновить продукт
    Task<bool> UpdateAsync(Guid id, ProductDto dto);

    // Удалить продукт
    Task<bool> DeleteAsync(Guid id);
}
