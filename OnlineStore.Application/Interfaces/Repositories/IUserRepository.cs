using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // Метод получения пользователя по его уникальному идентификатору
        Task<User?> GetByIdAsync(Guid id);

        // Метод получения пользователя по нормализованному email
        Task<User?> GetByNormalizedEmailAsync(string normalized);

        // Метод добавления нового пользователя
        Task AddAsync(User user);

        // Метод обновления хеша пароля пользователя
        // Используется при смене пароля для обновления хеша в базе данных
        Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash);
    }
}