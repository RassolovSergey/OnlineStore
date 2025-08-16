using Microsoft.Extensions.Configuration;
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

namespace OnlineStore.Infrastructure.Services;

public class AuthService : IAuthService
{
    // Репозиторий для работы с пользователями
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtOptions _jwt;
    private readonly ILogger<AuthService> _logger;

    // Получение профиля пользователя по его идентификатору
    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        // Ищем в репозитории
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("Пользователь не найден.");

        // Маппим руками (можно через AutoMapper при желании)
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    // Конструктор для внедрения зависимостей
    public AuthService(IUserRepository users, IPasswordHasher<User> passwordHasher, IOptions<JwtOptions> jwtOptions, ILogger<AuthService> logger)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwtOptions.Value;
        _logger = logger;
    }

    // Утилита нормализации email
    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустой строкой", nameof(email));

        return email.Trim().ToUpperInvariant();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Проверяем, что email не пустой
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email не может быть пустой строкой", nameof(request.Email));

        // Удаляем пробелы и нормализуем email
        var email = request.Email.Trim();
        var normalized = NormalizeEmail(email);

        // Проверяем уникальность именно по нормализованному email
        var existing = await _users.GetByNormalizedEmailAsync(normalized);
        if (existing != null)
            throw new Exception("Пользователь с таким email уже зарегистрирован");

        // Создаем нового пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalized,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = _passwordHasher.HashPassword(new User { Id = Guid.Empty, Email = email, NormalizedEmail = normalized }, request.Password)
        };

        // Добавляем пользователя в репозиторий и сохраняем изменения
        await _users.AddAsync(user);
        await _users.SaveChangesAsync();

        // Генерируем токен для нового пользователя
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Проверяем, что email не пустой
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email не может быть пустой строкой", nameof(request.Email));

        // Нормализуем email
        var normalized = NormalizeEmail(request.Email);

        // Ищем пользователя по нормализованному email
        var user = await _users.GetByNormalizedEmailAsync(normalized);

        // Единое сообщение для безопасности (не раскрываем, чего нет — пользователя или пароля)
        const string invalid = "Неверные учётные данные.";

        if (user == null)
        {
            _logger.LogWarning("Auth: login failed (user not found) for {Email}", normalized);
            throw new UnauthorizedAppException(invalid);
        }

        // Проверяем пароль
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        // Если пароль неверный, возвращаем единое сообщение
        if (result == PasswordVerificationResult.Failed)
        {
            // Логируем неудачную попытку входа
            _logger.LogWarning("Auth: login failed (bad password) for {Email}", normalized);
            // Бросаем исключение с единым сообщением
            throw new UnauthorizedAppException(invalid);
        }

        // Если алгоритм/параметры хеша устарели — пересчитаем и сохраним новый хеш
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // Обновляем хеш пароля пользователя
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            // Сохраняем изменения в репозитории
            await _users.SaveChangesAsync();
            // Логируем пересчет хеша пароля
            _logger.LogInformation("Auth: password rehashed for {UserId}", user.Id);
        }

        // Логируем успешный вход
        _logger.LogInformation("Auth: login success for {UserId}", user.Id);
        // Возвращаем ответ с токеном и email пользователя
        return GenerateAuthResponse(user);
    }

    // Метод для генерации JWT токена
    private AuthResponse GenerateAuthResponse(User user)
    {
        // Создаем список клаймов (claims) для токена
        // Используем стандартные клаймы JWT для идентификации пользователя
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Создаем ключ безопасности и подпись для токена
        // Используем секретный ключ из настроек JWT и алгоритм HMAC SHA256 для
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Устанавливаем время жизни токена
        // Используем значение из настроек JWT для установки времени жизни токена
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenLifetimeMinutes);

        // Создаем JWT токен с указанными клаймами, временем жизни и подписью
        // Используем конструктор JwtSecurityToken для создания токена
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        // Возвращаем объект AuthResponse с email пользователя и сериализованным токеном
        // Используем JwtSecurityTokenHandler для сериализации токена в строку
        return new AuthResponse
        {
            Email = user.Email,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }

    // Метод для смены пароля пользователя
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        // 1) Находим пользователя
        var user = await _users.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("Пользователь не найден.");

        // 2) Проверяем текущий пароль
        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Auth: change password failed (bad current) for {UserId}", userId);
            throw new UnauthorizedAppException("Неверные учётные данные."); // единое сообщение
        }

        // 3) Если алгоритм устарел — можно проигнорить тут; важнее — установить НОВЫЙ хеш
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);

        // (опционально) user.LastPasswordChangedAt = DateTime.UtcNow;

        await _users.SaveChangesAsync();
        _logger.LogInformation("Auth: password changed for {UserId}", userId);
    }


}
