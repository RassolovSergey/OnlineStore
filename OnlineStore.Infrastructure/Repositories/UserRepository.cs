using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Interfaces.Repositories;
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

    // Метод получения пользователя Id
    public async Task<User?> GetByIdAsync(Guid id)
    {
        // Поиск пользователя в базе данных по Id
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);  // Поиск пользователя по Id
    }

    // Метод получения пользователя по нормализованному email
    public async Task<User?> GetByNormalizedEmailAsync(string normalized)
    {
        return await _context.Users
            .AsNoTracking() // Используем AsNoTracking для повышения производительности при чтении
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized); // Поиск пользователя по нормализованному email
    }

    // Добавление нового пользователя
    public Task AddAsync(User user)
    {
        _context.Users.Add(user);
        return Task.CompletedTask;
    }

    // Обновление хеша пароля
    public async Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash)
    {
        var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        user.PasswordHash = newPasswordHash;
    }
}
