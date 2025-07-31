using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.DTOs;
using OnlineStore.Application.Interfaces;

namespace OnlineStore.Api.Controllers;

///   <summary>
///  Контроллер для работы с продуктами. 
///  <summary>
[ApiController] // Атрибуты контроллера
[Route("api/[controller]")] // Указываем маршрут для контроллера
public class ProductController : ControllerBase
{
    // Поле для сервиса продуктов
    private readonly IProductService _service;

    // Конструктор, принимающий IProductService
    public ProductController(IProductService service)
    {
        // Инициализируем сервис продуктов
        // Это позволяет использовать сервис для обработки бизнес-логики
        _service = service;
    }

    // Получить все продукты
    [HttpGet]   // Указывает, что метод будет обрабатывать HTTP-запросы типа GET.
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        // Получаем список всех продуктов через сервис
        var products = await _service.GetAllAsync();
        // Возвращаем результат в формате Ok (200 OK)
        return Ok(products);
    }

    // Получить продукт по ID
    [HttpGet("{id}")]   // Указывает, что метод принимает параметр id из URL и будет обрабатывать HTTP-запросы типа GET.
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        // Получаем продукт по ID через сервис
        var product = await _service.GetByIdAsync(id);
        // Если продукт не найден, возвращаем NotFound (404)
        if (product == null) return NotFound();

        // Возвращаем найденный продукт в формате Ok (200 OK)
        return Ok(product);
    }

    // Создать новый продукт
    [HttpPost]  // Указывает, что метод будет обрабатывать HTTP-запросы типа POST.
    public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
    {
        // Создаем новый продукт через сервис
        var created = await _service.CreateAsync(dto);
        // Возвращаем созданный продукт с кодом Created (201 Created)
        // Указываем маршрут для доступа к созданному продукту
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Обновить продукт
    [HttpPut("{id}")]   // Указывает, что метод принимает параметр id из URL и будет обрабатывать HTTP-запросы типа PUT.
    public async Task<IActionResult> Update(Guid id, ProductDto dto)
    {
        // Обновляем продукт через сервис
        var updated = await _service.UpdateAsync(id, dto);
        // Если продукт не найден, возвращаем NotFound (404)
        if (!updated) return NotFound();

        // Возвращаем NoContent (204 No Content) при успешном обновлении
        return NoContent();
    }

    // Удалить продукт
    [HttpDelete("{id}")]    // Указывает, что метод принимает параметр id из URL и будет обрабатывать HTTP-запросы типа DELETE.
    public async Task<IActionResult> Delete(Guid id)
    {
        // Удаляем продукт через сервис
        var deleted = await _service.DeleteAsync(id);
        // Если продукт не найден, возвращаем NotFound (404)
        if (!deleted) return NotFound();

        // Возвращаем NoContent (204 No Content) при успешном удалении
        return NoContent();
    }
}
