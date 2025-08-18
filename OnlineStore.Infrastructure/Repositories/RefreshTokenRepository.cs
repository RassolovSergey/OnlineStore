using Microsoft.EntityFrameworkCore;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;
using OnlineStore.Application.Interfaces.Repositories;

namespace OnlineStore.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;
        public RefreshTokenRepository(AppDbContext db) => _db = db;

        public Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
            _db.Set<RefreshToken>().FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        // Выдать токен
        public Task AddAsync(RefreshToken token)
        {
            _db.Set<RefreshToken>().Add(token);
            return Task.CompletedTask;
        }

        // Отозвать токен
        public Task RevokeAsync(RefreshToken token, string? ip = null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ip;
            return Task.CompletedTask;
        }

        // Метод сохранения, на случай, если где-то нужно локально досохранить.
        public async Task SaveAsync() => await _db.SaveChangesAsync();

        // Отозвать все Токены
        public async Task RevokeAllForUserAsync(Guid userId, string? ip = null)
        {
            var now = DateTime.UtcNow;
            await _db.Set<RefreshToken>()
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
                .ExecuteUpdateAsync(up => up
                    .SetProperty(t => t.RevokedAt, now)
                    .SetProperty(t => t.RevokedByIp, ip));
        }
    }
}