using AutoMapper;
using OnlineStore.Application.DTOs;
using OnlineStore.Application.Interfaces;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Services;

/// <summary>
/// Сервис для работы с продуктами с использованием AutoMapper.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;    // Репозиторий для доступа к продуктам
    private readonly IMapper _mapper;   // AutoMapper для преобразования между сущностями и DTO

    // Конструктор, принимающий репозиторий и маппер
    public ProductService(IProductRepository repository, IMapper mapper)
    {
        _repository = repository; // Инициализируем репозиторий для доступа к продуктам
        _mapper = mapper;  // Инициализируем AutoMapper для преобразования между сущностями и DTO
    }

    // Методы сервиса для работы с продуктами
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        // Получаем все продукты из репозитория и преобразуем их в DTO
        var products = await _repository.GetAllAsync();
        // Используем AutoMapper для преобразования списка сущностей Product в список ProductDto
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // Получить продукт по идентификатору
    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        // Получаем продукт по ID из репозитория
        var product = await _repository.GetByIdAsync(id);
        // Используем AutoMapper для преобразования сущности Product в ProductDto
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }
    // Создать продукт
    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        // Используем AutoMapper для преобразования ProductDto в сущность Product
        var product = _mapper.Map<Product>(dto);
        // Генерируем новый ID для продукта, если он не задан
        product.Id = Guid.NewGuid();

        // Добавляем новый продукт в репозиторий
        await _repository.AddAsync(product);
        // Возвращаем созданный продукт в виде DTO
        return _mapper.Map<ProductDto>(product);
    }
    // Обновить продукт
    public async Task<bool> UpdateAsync(Guid id, ProductDto dto)
    {
        // Обрабатываем обновление продукта по ID
        var product = await _repository.GetByIdAsync(id);
        // Если продукт не найден, возвращаем false
        if (product == null) return false;

        // Используем AutoMapper для обновления полей продукта из DTO
        _mapper.Map(dto, product);
        // Сохраняем изменения в репозитории
        await _repository.UpdateAsync(product);
        // Возвращаем true, если обновление прошло успешно
        return true;
    }
    // Удалить продукт
    public async Task<bool> DeleteAsync(Guid id)
    {
        // Получаем продукт по ID из репозитория
        var product = await _repository.GetByIdAsync(id);
        // Если продукт не найден, возвращаем false
        if (product == null) return false;

        // Удаляем продукт из репозитория
        await _repository.DeleteAsync(product);
        // Возвращаем true, если удаление прошло успешно
        return true;
    }
}
