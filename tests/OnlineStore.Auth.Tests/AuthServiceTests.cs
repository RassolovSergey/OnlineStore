using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Exceptions;
using OnlineStore.Application.Interfaces.Repositories;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Options;
using OnlineStore.Infrastructure.Persistence;
using OnlineStore.Infrastructure.Security;
using OnlineStore.Infrastructure.Services;

namespace OnlineStore.Auth.Tests;

/// <summary>
/// Юнит-тесты бизнес-логики аутентификации (AuthService).
/// Здесь мы всегда используем SQLite In-Memory DbContext (без EnsureCreated),
/// чтобы работали транзакции и SaveChangesAsync, но без реальной схемы БД.
/// </summary>
public class AuthServiceTests
{
    private static JwtOptions Jwt() => new JwtOptions
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Key = new string('K', 64),              // ключ для подписи
        AccessTokenLifetimeMinutes = 60,
        RefreshTokenLifetimeDays = 30
    };

    // ПУНКТ 3: единая фабрика — ВСЕГДА SQLite InMemory (без EnsureCreated)
    private static (
        AuthService Svc,
        Mock<IUserRepository> Users,
        Mock<IRefreshTokenRepository> Tokens,
        Mock<IPasswordHasher<User>> Hasher,
        Mock<IRefreshTokenFactory> TokenFactory,
        AppDbContext Db,
        Mock<ILogger<AuthService>> Log,
        IDisposable? Disposable
    ) BuildService()
    {
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        var hasher = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);
        var tokenFactory = new Mock<IRefreshTokenFactory>(MockBehavior.Strict);
        var logger = new Mock<ILogger<AuthService>>();

        // важное: создаём реляционный провайдер (SQLite In-Memory), без EnsureCreated
        var pair = TestHelpers.CreateSqliteDb(ensureCreated: false);
        var db = pair.Db;
        IDisposable? disp = pair.Conn;

        var svc = new AuthService(
            users.Object,
            hasher.Object,
            Options.Create(Jwt()),
            logger.Object,
            tokenFactory.Object,
            tokens.Object,
            db
        );

        return (svc, users, tokens, hasher, tokenFactory, db, logger, disp);
    }

    [Fact]
    public async Task Register_should_throw_Conflict_if_email_exists()
    {
        var (svc, users, _, _, _, db, _, disp) = BuildService();
        try
        {
            users.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>()))
                 .ReturnsAsync(new User()); // уже есть

            var req = new RegisterRequest { Email = "user@example.com", Password = "Strong9!" };

            await FluentActions.Invoking(() => svc.RegisterAsync(req))
                .Should().ThrowAsync<ConflictAppException>(); // 409
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Register_should_create_user_and_return_access_and_refresh()
    {
        var (svc, users, tokens, hasher, factory, db, _, disp) = BuildService();
        try
        {
            users.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>()))
                 .ReturnsAsync((User?)null);          // нет коллизии
            users.Setup(r => r.AddAsync(It.IsAny<User>()))
                 .Returns(Task.CompletedTask);

            hasher.Setup(h => h.HashPassword(It.IsAny<User>(), "Strong9!"))
                  .Returns("HASH");

            var rawRefresh = "raw-rt";
            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TokenHash = "HASHED",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            factory.Setup(f => f.Create(It.IsAny<Guid>(), null, null))
                   .Returns((rawRefresh, entity));

            tokens.Setup(t => t.AddAsync(entity))
                  .Returns(Task.CompletedTask);

            var res = await svc.RegisterAsync(new RegisterRequest { Email = "user@example.com", Password = "Strong9!" });

            res.Email.Should().Be("user@example.com");
            res.Token.Should().NotBeNullOrWhiteSpace();
            res.RefreshToken.Should().Be(rawRefresh);

            // Разберём JWT и проверим наличие claim email
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(res.Token);
            jwt.Claims.Select(c => c.Type).Should().Contain(JwtRegisteredClaimNames.Email);
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Login_should_throw_Unauthorized_on_missing_user_or_bad_password()
    {
        var (svc, users, _, hasher, _, db, _, disp) = BuildService();
        try
        {
            users.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>()))
                 .ReturnsAsync((User?)null);

            await FluentActions.Invoking(() => svc.LoginAsync(new LoginRequest { Email = "no@no", Password = "x" }))
                .Should().ThrowAsync<UnauthorizedAppException>();

            var u = new User { Id = Guid.NewGuid(), Email = "a@b", PasswordHash = "PH" };
            users.Reset();
            users.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>())).ReturnsAsync(u);
            hasher.Setup(h => h.VerifyHashedPassword(u, "PH", "x")).Returns(PasswordVerificationResult.Failed);

            await FluentActions.Invoking(() => svc.LoginAsync(new LoginRequest { Email = "a@b", Password = "x" }))
                .Should().ThrowAsync<UnauthorizedAppException>();
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Login_should_return_tokens_and_rehash_when_needed()
    {
        var (svc, users, tokens, hasher, factory, db, _, disp) = BuildService();
        try
        {
            var u = new User { Id = Guid.NewGuid(), Email = "a@b", PasswordHash = "OLD", IsAdmin = true };
            users.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>())).ReturnsAsync(u);

            hasher.Setup(h => h.VerifyHashedPassword(u, "OLD", "pass"))
                  .Returns(PasswordVerificationResult.SuccessRehashNeeded);
            hasher.Setup(h => h.HashPassword(u, "pass")).Returns("NEW");

            var rawRt = "rt-raw";
            var ent = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                TokenHash = "H",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            factory.Setup(f => f.Create(u.Id, null, null)).Returns((rawRt, ent));
            tokens.Setup(t => t.AddAsync(ent)).Returns(Task.CompletedTask);

            var res = await svc.LoginAsync(new LoginRequest { Email = "a@b", Password = "pass" });

            res.Email.Should().Be("a@b");
            res.RefreshToken.Should().Be(rawRt);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(res.Token);

            jwt.Claims.Any(c =>
                   (c.Type == "role" || c.Type == ClaimTypes.Role)   // поддерживаем оба варианта
                && c.Value == "Admin")
                .Should().BeTrue();
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Refresh_should_rotate_and_return_new_access()
    {
        var (svc, users, tokens, _, factory, db, _, disp) = BuildService();
        try
        {
            var uid = Guid.NewGuid();
            var u = new User { Id = uid, Email = "u@e" };
            users.Setup(r => r.GetByIdAsync(uid)).ReturnsAsync(u);

            var existing = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = uid,
                TokenHash = "HX",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            factory.Setup(f => f.Hash("raw-in")).Returns("HX");
            tokens.Setup(t => t.GetByHashAsync("HX")).ReturnsAsync(existing);

            // ПУНКТ 4: сервис обычно отзывает старый токен — добавляем setup
            tokens.Setup(t => t.RevokeAsync(existing, "1.2.3.4")).Returns(Task.CompletedTask);

            var rawOut = "raw-out";
            var newEnt = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = uid,
                TokenHash = "NEW",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            factory.Setup(f => f.Create(uid, "1.2.3.4", "UA")).Returns((rawOut, newEnt));
            tokens.Setup(t => t.AddAsync(newEnt)).Returns(Task.CompletedTask);

            var res = await svc.RefreshAsync(new RefreshRequest { RefreshToken = "raw-in" }, "1.2.3.4", "UA");

            res.Email.Should().Be("u@e");
            res.RefreshToken.Should().Be(rawOut);
            existing.ReplacedByTokenId.Should().Be(newEnt.Id); // связка событий ротации
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Refresh_should_revoke_all_on_inactive_token()
    {
        var (svc, _, tokens, _, factory, db, _, disp) = BuildService();
        try
        {
            var expired = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TokenHash = "HX",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                RevokedAt = DateTime.UtcNow
            };

            factory.Setup(f => f.Hash("raw")).Returns("HX");
            tokens.Setup(t => t.GetByHashAsync("HX")).ReturnsAsync(expired);
            tokens.Setup(t => t.RevokeAllForUserAsync(expired.UserId, "ip")).Returns(Task.CompletedTask);

            await FluentActions.Invoking(() => svc.RefreshAsync(new RefreshRequest { RefreshToken = "raw" }, "ip", "ua"))
                .Should().ThrowAsync<UnauthorizedAppException>();
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Logout_variants_should_call_repositories_as_expected()
    {
        var (svc, _, tokens, _, factory, db, _, disp) = BuildService();
        try
        {
            // Empty input
            await svc.LogoutAsync("");
            Mock.Get(tokens.Object).Verify(r => r.GetByHashAsync(It.IsAny<string>()), Times.Never);

            // Unknown token
            factory.Setup(f => f.Hash("x")).Returns("HX");
            tokens.Setup(r => r.GetByHashAsync("HX")).ReturnsAsync((RefreshToken?)null);
            await svc.LogoutAsync("x");
            Mock.Get(tokens.Object).Verify(r => r.RevokeAsync(It.IsAny<RefreshToken>(), It.IsAny<string?>()), Times.Never);

            // Active token
            var t = new RefreshToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ExpiresAt = DateTime.UtcNow.AddDays(1) };
            tokens.Reset();
            factory.Setup(f => f.Hash("y")).Returns("HY");
            tokens.Setup(r => r.GetByHashAsync("HY")).ReturnsAsync(t);
            tokens.Setup(r => r.RevokeAsync(t, "ip")).Returns(Task.CompletedTask);

            await svc.LogoutAsync("y", "ip");
            Mock.Get(tokens.Object).Verify(r => r.RevokeAsync(t, "ip"), Times.Once);
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task GetProfile_should_throw_when_not_found_and_return_dto_when_ok()
    {
        var (svc, users, _, _, _, db, _, disp) = BuildService();
        try
        {
            users.Setup(u => u.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);
            await FluentActions.Invoking(() => svc.GetProfileAsync(Guid.NewGuid())).Should().ThrowAsync<NotFoundException>();

            var u = new User { Id = Guid.NewGuid(), Email = "x@y", CreatedAt = new DateTime(2020, 1, 1) };
            users.Reset();
            users.Setup(x => x.GetByIdAsync(u.Id)).ReturnsAsync(u);
            var dto = await svc.GetProfileAsync(u.Id);
            dto.Email.Should().Be("x@y");
            dto.Id.Should().Be(u.Id);
            dto.CreatedAt.Should().Be(u.CreatedAt);
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task ChangePassword_should_verify_current_and_set_new_hash()
    {
        var (svc, users, _, hasher, _, db, _, disp) = BuildService();
        try
        {
            var u = new User { Id = Guid.NewGuid(), Email = "x@y", PasswordHash = "OLD" };
            users.Setup(r => r.GetByIdAsync(u.Id)).ReturnsAsync(u);

            hasher.Setup(h => h.VerifyHashedPassword(u, "OLD", "cur")).Returns(PasswordVerificationResult.Success);
            hasher.Setup(h => h.HashPassword(u, "new")).Returns("NEW");

            await svc.ChangePasswordAsync(u.Id, new ChangePasswordRequest { CurrentPassword = "cur", NewPassword = "new" }, CancellationToken.None);

            u.PasswordHash.Should().Be("NEW");
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }

    [Fact]
    public async Task Sessions_and_logout_session_should_work()
    {
        var (svc, _, tokens, _, _, db, _, disp) = BuildService();
        try
        {
            var uid = Guid.NewGuid();
            var list = new List<RefreshToken>
            {
                new() { Id = Guid.NewGuid(), UserId = uid, CreatedAt = DateTime.UtcNow.AddHours(-1), ExpiresAt = DateTime.UtcNow.AddDays(1), CreatedByIp = "1", CreatedByUa = "UA" }
            }.AsReadOnly();

            tokens.Setup(r => r.GetActiveByUserAsync(uid, It.IsAny<CancellationToken>())).ReturnsAsync(list);
            var dtos = await svc.GetSessionsAsync(uid);
            dtos.Should().HaveCount(1);
            dtos.Single().IsActive.Should().BeTrue();

            var token = new RefreshToken { Id = Guid.NewGuid(), UserId = uid, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            tokens.Setup(r => r.GetByIdAsync(token.Id, It.IsAny<CancellationToken>())).ReturnsAsync(token);
            tokens.Setup(r => r.RevokeAsync(token, "ip")).Returns(Task.CompletedTask);

            await svc.LogoutSessionAsync(uid, token.Id, "ip");
        }
        finally { disp?.Dispose(); db.Dispose(); }
    }
}