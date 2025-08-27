using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OnlineStore.Application.Exceptions;
using OnlineStore.Application.Interfaces.Repositories;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Services
{
    /// <summary>
    /// Сервис административных операций над пользователями.
    /// Сохраняем изменения единым вызовом DbContext.SaveChangesAsync().
    /// </summary>
    public sealed class UsersService : IUsersService
    {
        // Поля для работы с зависимостями
        private readonly IUserRepository _users;
        private readonly AppDbContext _db;
        private readonly ILogger<UsersService> _logger;

        //  Конструктор с внедрением зависимостей
        public UsersService(IUserRepository users, AppDbContext db, ILogger<UsersService> logger)
        {
            _users = users;
            _db = db;
            _logger = logger;
        }

        // Метод: Добавить пользователи права администратора
        public async Task MakeAdminAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден.");
            if (user.IsAdmin) return;

            user.IsAdmin = true;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Users: made admin {UserId}", userId);
        }

        // Метод: Забрать у пользователя права администратора
        public async Task RemoveAdminAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден.");
            if (!user.IsAdmin) return;

            user.IsAdmin = false;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Users: removed admin {UserId}", userId);
        }

        // Метод: Мягкое удаление пользователя (без физического удаления из БД)
        public async Task SoftDeleteAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден.");
            if (user.IsDeleted) return;

            user.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Users: soft-deleted {UserId}", userId);
        }

        // Метод: Восстановление пользователя
        public async Task RestoreAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId) ?? throw new NotFoundException("Пользователь не найден.");
            if (!user.IsDeleted) return;

            user.IsDeleted = false;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Users: restored {UserId}", userId);
        }
    }
}
