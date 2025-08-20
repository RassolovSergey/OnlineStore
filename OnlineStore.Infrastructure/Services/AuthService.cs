using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using OnlineStore.Application.Exceptions;
using Microsoft.Extensions.Options;
using OnlineStore.Infrastructure.Options;
using OnlineStore.Infrastructure.Persistence;
using OnlineStore.Infrastructure.Security;
using OnlineStore.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace OnlineStore.Infrastructure.Services
{
    /// <summary>
    /// Сервис аутентификации: регистрация/логин/JWT/refresh-поток/смена пароля.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;                     // Репозиторий пользователей
        private readonly IPasswordHasher<User> _passwordHasher;      // Хешер паролей (Identity)
        private readonly JwtOptions _jwt;                            // Опции JWT
        private readonly ILogger<AuthService> _logger;               // Логер
        private readonly IRefreshTokenFactory _rtFactory;            // Фабрика refresh-токенов
        private readonly IRefreshTokenRepository _rtRepo;            // Репозиторий refresh-токенов
        private readonly AppDbContext _db;                           // EF Core DbContext для транзакций/коммитов

        public AuthService(
            IUserRepository users,
            IPasswordHasher<User> passwordHasher,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AuthService> logger,
            IRefreshTokenFactory rtFactory,
            IRefreshTokenRepository rtRepo,
            AppDbContext db)
        {
            _users = users;
            _passwordHasher = passwordHasher;
            _jwt = jwtOptions.Value;
            _logger = logger;
            _rtFactory = rtFactory;
            _rtRepo = rtRepo;
            _db = db;
        }

        // Единая нормализация email для поиска/уникальности
        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email не может быть пустой строкой", nameof(email));

            return email.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Регистрация нового пользователя + выпуск пары токенов (access + refresh) атомарно.
        /// </summary>
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email не может быть пустой строкой", nameof(request.Email));

            var email = request.Email.Trim();
            var normalized = NormalizeEmail(email);

            // Проверяем уникальность
            var existing = await _users.GetByNormalizedEmailAsync(normalized);
            if (existing is not null)
                throw new ConflictAppException("Пользователь с таким email уже зарегистрирован.");

            // Готовим сущность пользователя
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                NormalizedEmail = normalized,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Добавляем пользователя (без SaveChanges — коммитим в конце)
                await _users.AddAsync(user);

                // Генерируем access JWT
                var access = GenerateJwt(user);

                // Создаём refresh-токен (ВАЖНО: позиционные аргументы, без имён)
                // Сигнатура фабрики: скорее всего (Guid userId, string? ip, string? ua)
                var (rawRefresh, refreshEntity) = _rtFactory.Create(user.Id, null, null);

                // Сохраняем refresh-токен (без SaveChanges здесь)
                await _rtRepo.AddAsync(refreshEntity);

                // ЕДИНЫЙ коммит и завершение транзакции
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation("Auth: user registered {UserId}", user.Id);

                return new AuthResponse
                {
                    Email = user.Email,
                    Token = access,
                    RefreshToken = rawRefresh
                };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Auth: registration failed for {Email}", SafeLog.MaskEmail(email));
                throw;
            }
        }

        /// <summary>
        /// Авторизация по email/паролю. При устаревшем хеше — перехэш пароля. Возвращает пару токенов.
        /// </summary>
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email не может быть пустой строкой", nameof(request.Email));

            var normalized = NormalizeEmail(request.Email);
            var user = await _users.GetByNormalizedEmailAsync(normalized);

            const string invalid = "Неверные учётные данные.";
            if (user is null)
            {
                _logger.LogWarning("Auth: login failed (user not found) for {Email}", SafeLog.MaskEmail(request.Email));
                throw new UnauthorizedAppException(invalid);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Auth: login failed (bad password) for {Email}", SafeLog.MaskEmail(request.Email));
                throw new UnauthorizedAppException(invalid);
            }

            // Если хеш устарел — обновляем его и сохраняем
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                await _db.SaveChangesAsync();
            }

            // Выпускаем access
            var access = GenerateJwt(user);

            // И создаём refresh (позиционно)
            var (rawRefresh, refreshEntity) = _rtFactory.Create(user.Id, null, null);
            await _rtRepo.AddAsync(refreshEntity);

            // Коммитим один раз
            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                Email = user.Email,
                Token = access,
                RefreshToken = rawRefresh
            };
        }

        /// <summary>
        /// Профиль текущего пользователя.
        /// </summary>
        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _users.GetByIdAsync(userId)
                       ?? throw new NotFoundException("Пользователь не найден.");

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        /// <summary>
        /// Смена пароля после проверки текущего пароля.
        /// </summary>
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(userId)
                       ?? throw new NotFoundException("Пользователь не найден.");

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (verify == PasswordVerificationResult.Failed)
                throw new UnauthorizedAppException("Неверные учётные данные.");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

            await _db.SaveChangesAsync();
            _logger.LogInformation("Auth: password changed for {UserId}", userId);
        }

        /// <summary>
        /// Обновление пары токенов по действующему refresh-токену (ротация).
        /// </summary>
        public async Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip = null, string? ua = null)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new UnauthorizedAppException("Неверные учётные данные.");

            // Сверяем по хешу refresh-токена
            var hash = _rtFactory.Hash(request.RefreshToken);
            var token = await _rtRepo.GetByHashAsync(hash)
                ?? throw new UnauthorizedAppException("Неверные учётные данные.");

            // Если уже отозван/просрочен — жёсткая защита: отзывать все активные у пользователя
            if (!token.IsActive)
            {
                await _rtRepo.RevokeAllForUserAsync(token.UserId, ip);
                throw new UnauthorizedAppException("Неверные учётные данные.");
            }

            // Ротация: текущий refresh → отозвать
            await _rtRepo.RevokeAsync(token, ip);

            // Достаём пользователя
            var user = await _users.GetByIdAsync(token.UserId)
                       ?? throw new NotFoundException("Пользователь не найден.");

            // Новый access
            var access = GenerateJwt(user);

            // Новый refresh (позиционные аргументы ip/ua)
            var (rawNew, newEntity) = _rtFactory.Create(user.Id, ip, ua);
            await _rtRepo.AddAsync(newEntity);

            // Связь цепочки ротации
            token.ReplacedByTokenId = newEntity.Id;

            // Коммитим изменения
            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                Email = user.Email,
                Token = access,
                RefreshToken = rawNew
            };
        }

        /// <summary>
        /// Отзыв конкретного refresh-токена (logout). Идемпотентно.
        /// </summary>
        public async Task LogoutAsync(string refreshToken, string? ip = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return;

            var hash = _rtFactory.Hash(refreshToken);
            var entity = await _rtRepo.GetByHashAsync(hash);
            if (entity is null) return;

            if (entity.RevokedAt is null)
            {
                await _rtRepo.RevokeAsync(entity, ip);
                await _db.SaveChangesAsync(); // фикс: надо сохранить отзыв
            }
        }

        /// <summary>
        /// Вспомогательный метод: выпускает пару токенов для пользователя (access + refresh).
        /// Не используется снаружи, но оставлен для удобства.
        /// </summary>
        private async Task<AuthResponse> IssueTokensAsync(User user, string? requestIp, string? requestUa)
        {
            // Access JWT
            var access = GenerateJwt(user);

            // Refresh (позиционно)
            var (rawRefresh, entity) = _rtFactory.Create(user.Id, requestIp, requestUa);
            await _rtRepo.AddAsync(entity);

            return new AuthResponse
            {
                Email = user.Email,
                Token = access,
                RefreshToken = rawRefresh
            };
        }

        /// <summary>
        /// Генерация access-JWT для пользователя.
        /// </summary>
        private string GenerateJwt(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (user.IsAdmin)
            {
                // Ключевой момент для [Authorize(Roles="Admin")]
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenLifetimeMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId)
        {
            var list = await _rtRepo.GetActiveByUserAsync(userId); // добавим в репозиторий
            return list.Select(t => new SessionDto
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                CreatedByIp = t.CreatedByIp,
                CreatedByUa = t.CreatedByUa,
                IsActive = t.IsActive,
                IsCurrent = false // отметим в контроллере
            }).ToList();
        }

        public async Task LogoutAllAsync(Guid userId, string? ip = null)
        {
            await _rtRepo.RevokeAllForUserAsync(userId, ip);
            await _db.SaveChangesAsync();
        }

        public async Task LogoutSessionAsync(Guid userId, Guid tokenId, string? ip = null)
        {
            var token = await _rtRepo.GetByIdAsync(tokenId)
                ?? throw new NotFoundException("Сессия не найдена.");

            if (token.UserId != userId)
                throw new UnauthorizedAppException("Доступ запрещён.");

            if (token.RevokedAt is null)
                await _rtRepo.RevokeAsync(token, ip);

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Вернуть список активных сессий (refresh-токенов) пользователя.
        /// </summary>
        public async Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken ct = default)
        {
            // Проверим, что пользователь существует (чтобы 404 был корректный)
            var user = await _users.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException("Пользователь не найден.");

            var tokens = await _rtRepo.GetActiveByUserAsync(userId, ct);

            // Проекция в DTO
            var list = tokens.Select(t => new SessionDto
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                CreatedByIp = t.CreatedByIp,
                CreatedByUa = t.CreatedByUa,
                IsActive = t.RevokedAt is null && t.ExpiresAt > DateTime.UtcNow,
                IsCurrent = false // на шаге 4 можно отметить "текущую" по cookie
            }).ToList();

            return list;
        }
        /// <summary>
        /// Закрыть все сессии пользователя.
        /// </summary>
        public async Task LogoutAllAsync(Guid userId, string? ip = null, CancellationToken ct = default)
        {
            // Дополнительно можно проверить владельца/права выше, в контроллере
            await _rtRepo.RevokeAllForUserAsync(userId, ip);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Auth: revoked ALL sessions for {UserId} (ip: {IP})", userId, ip);
        }

        /// <summary>
        /// Закрыть конкретную сессию пользователя (по Id refresh-токена).
        /// </summary>
        public async Task LogoutSessionAsync(Guid userId, Guid tokenId, string? ip = null, CancellationToken ct = default)
        {
            var token = await _rtRepo.GetByIdAsync(tokenId, ct)
                        ?? throw new NotFoundException("Сессия не найдена.");

            // Владелец должен совпадать
            if (token.UserId != userId)
                throw new UnauthorizedAppException("Доступ запрещён.");

            if (token.RevokedAt is null)
            {
                await _rtRepo.RevokeAsync(token, ip);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Auth: revoked session {TokenId} for {UserId} (ip: {IP})", tokenId, userId, ip);
            }
        }
    }
}
