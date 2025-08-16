using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces.Repositories;

public interface IUserRepository
{
    // Метод получения пользователя по его уникальному идентификатору
    Task<User?> GetByIdAsync(Guid id);

    //  Метод получения пользователя по email
    Task<User?> GetByEmailAsync(string email);

    // Метод получения пользователя по нормализованному email
    Task<User?> GetByNormalizedEmailAsync(string normalized);

    // Метод добавления нового пользователя
    Task AddAsync(User user);

    // Метод сохранения изменений в репозитории
    Task SaveChangesAsync(CancellationToken ct = default);
}
