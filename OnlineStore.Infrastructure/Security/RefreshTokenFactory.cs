using System.Security.Cryptography;
using System.Text;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace OnlineStore.Infrastructure.Security
{
    public interface IRefreshTokenFactory
    {
        (string Raw, RefreshToken Entity) Create(Guid userId, string? ip, string? ua);
        string Hash(string raw);
    }

    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        private readonly JwtOptions _jwt;
        public RefreshTokenFactory(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

        public (string Raw, RefreshToken Entity) Create(Guid userId, string? ip, string? ua)
        {
            // Генерируем крипто-устойчивую строку
            var bytes = RandomNumberGenerator.GetBytes(64);
            var raw = Convert.ToBase64String(bytes); // «сырой» RT, попадёт клиенту

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = Hash(raw),                        // в БД храним только хеш
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenLifetimeDays),
                CreatedByIp = ip,
                CreatedByUa = ua
            };

            return (raw, entity);
        }

        public string Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}
