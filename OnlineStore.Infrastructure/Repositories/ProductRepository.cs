using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Interfaces;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с сущностью Product.
/// Использует EF Core и DbContext для доступа к базе данных.
/// </summary>
public class ProductRepository : IProductRepository
{
    // Контекст базы данных для доступа к продуктам
    private readonly AppDbContext _context;

    // Конструктор, принимающий контекст базы данных
    // для внедрения зависимостей
    public ProductRepository(AppDbContext context)
    {
        // Инициализируем контекст базы данных
        // Это позволяет использовать EF Core для работы с продуктами
        _context = context;
    }

    /// <summary>
    /// Получить список всех продуктов.
    /// </summary>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        // Используем EF Core для получения всех продуктов из базы данных
        // Возвращаем список продуктов
        return await _context.Products.ToListAsync();
    }

    /// <summary>
    /// Получить продукт по идентификатору.
    /// </summary>
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        // Используем EF Core для поиска продукта по ID
        // Возвращаем найденный продукт или null, если не найден
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Добавить новый продукт в базу данных.
    /// </summary>
    public async Task AddAsync(Product product)
    {
        // Используем EF Core для добавления нового продукта
        _context.Products.Add(product);
        // Асинхронно сохраняем изменения
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновить существующий продукт.
    /// </summary>
    public async Task UpdateAsync(Product product)
    {
        // Используем EF Core для обновления продукта
        _context.Products.Update(product);
        // Асинхронно сохраняем изменения в базе данных
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Удалить продукт.
    /// </summary>
    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
