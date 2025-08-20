using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken entity);
        Task<RefreshToken?> GetByHashAsync(string hash);
        Task RevokeAsync(RefreshToken token, string? ip);
        Task RevokeAllForUserAsync(Guid userId, string? ip);
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);
        Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
