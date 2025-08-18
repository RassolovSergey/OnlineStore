using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByHashAsync(string tokenHash);
        Task RevokeAsync(RefreshToken token, string? ip = null);
        Task SaveAsync();
        Task RevokeAllForUserAsync(Guid userId, string? ip = null);
    }
}
