using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Interfaces.Repositories;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий refresh-токенов. Хранит только ХЭШ токена.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db) => _db = db;

        /// <summary>
        /// Добавить новый refresh-токен (entity содержит только hash и метаданные).
        /// SaveChanges выполняется на уровне сервиса.
        /// </summary>
        public async Task AddAsync(RefreshToken entity)
        {
            await _db.RefreshTokens.AddAsync(entity);
        }

        /// <summary>
        /// Поиск по хэшу (используется при refresh/login/register).
        /// </summary>
        public async Task<RefreshToken?> GetByHashAsync(string hash)
        {
            // IgnoreQueryFilters НЕ нужен: токены удалённых пользователей нам не нужны для user-потока.
            return await _db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TokenHash == hash);
        }

        /// <summary>
        /// Получить токен по первичному ключу (для закрытия конкретной сессии в профиле).
        /// </summary>
        public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        /// <summary>
        /// Список активных сессий пользователя (для UI): не отозваны и не истекли.
        /// </summary>
        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _db.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Отозвать один refresh-токен (идемпотентно).
        /// </summary>
        public Task RevokeAsync(RefreshToken token, string? ip)
        {
            if (token.RevokedAt is null)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ip;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Отозвать ВСЕ refresh-токены пользователя. 
        /// ВАЖНО: .IgnoreQueryFilters(), если user уже IsDeleted=true.
        /// </summary>
        public async Task RevokeAllForUserAsync(Guid userId, string? ip)
        {
            var now = DateTime.UtcNow;

            await _db.RefreshTokens
                .IgnoreQueryFilters() // чтобы не потерять токены soft-deleted пользователя
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.RevokedAt, now)
                    .SetProperty(t => t.RevokedByIp, ip));
        }
    }
}