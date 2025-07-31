using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces;

/// <summary>
/// Интерфейс для работы с пользователями в базе данных.
/// </summary>
public interface IUserRepository
{
    // Получить пользователя по email
    Task<User?> GetByEmailAsync(string email);

    // Добавить нового пользователя
    Task AddAsync(User user);
}
