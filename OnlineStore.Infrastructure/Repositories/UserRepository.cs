using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Interfaces;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория пользователя через EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    // Конструктор для внедрения зависимостей
    public UserRepository(AppDbContext context)
    {
        // Инициализация контекста базы данных
        _context = context;
    }

    // Получение пользователя по email
    public async Task<User?> GetByEmailAsync(string email)
    {
        // Поиск пользователя в базе данных по email
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    }

    // Добавление нового пользователя
    public async Task AddAsync(User user)
    {
        // Добавление пользователя в контекст и сохранение изменений
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
