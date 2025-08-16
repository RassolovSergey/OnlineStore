using Microsoft.EntityFrameworkCore;

using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория пользователя через EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    // Контекст базы данных для работы с сущностями
    private readonly AppDbContext _context;

    // Конструктор для внедрения зависимостей
    public UserRepository(AppDbContext context)
    {
        // Инициализация контекста базы данных
        _context = context;
    }

    // Метод получения пользователя по его уникальному идентификатору
    public async Task<User?> GetByIdAsync(Guid id)
    {
        // Поиск пользователя в базе данных по Id
        return await _context.Users
            .AsNoTracking() // Используем AsNoTracking для повышения производительности при чтении
            .FirstOrDefaultAsync(u => u.Id == id);  // Поиск пользователя по Id
    }

    // Метод получения пользователя по email
    public async Task<User?> GetByEmailAsync(string email)
    {
        // Поиск пользователя в базе данных по email
        return await _context.Users
            .AsNoTracking() // Используем AsNoTracking для повышения производительности при чтении
            .FirstOrDefaultAsync(u => u.Email == email);    // Поиск пользователя по email
    }

    // Метод получения пользователя по нормализованному email
    public async Task<User?> GetByNormalizedEmailAsync(string normalized)
    {
        return await _context.Users
            .AsNoTracking() // Используем AsNoTracking для повышения производительности при чтении
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized); // Поиск пользователя по нормализованному email
    }

    // Добавление нового пользователя
    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);   // Добавляем пользователя в контекст
        await _context.SaveChangesAsync();  // Сохраняем изменения в базе данных
    }

    // Метод сохранения изменений в репозитории
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // Сохраняем изменения в контексте базы данных
        await _context.SaveChangesAsync(ct);
    }
}
