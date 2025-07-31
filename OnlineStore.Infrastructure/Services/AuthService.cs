using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces;
using OnlineStore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineStore.Infrastructure.Services;

/// <summary>
/// Реализация сервиса аутентификации с использованием JWT.
/// </summary>
public class AuthService : IAuthService
{
    // Зависимости: репозиторий пользователей, хеширование паролей, конфигурация
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    // Конструктор для внедрения зависимостей
    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _userRepository = userRepository; // Позволяет работать с объектами User
        _passwordHasher = passwordHasher; // Позволяет хешировать пароли
        _configuration = configuration; // Позволяет получать настройки из конфигурации
    }

    // Методы для регистрации и логина пользователей
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Проверка: пользователь с таким email уже существует?
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        // Если существует, выбрасываем исключение
        if (existingUser != null)
            throw new Exception("Пользователь с таким email уже зарегистрирован");

        // Иначе: Создаем нового пользователя
        var user = new User
        {
            Id = Guid.NewGuid(),    // Генерируем уникальный идентификатор
            Email = request.Email,  // Устанавливаем email из запроса
            CreatedAt = DateTime.UtcNow // Устанавливаем дату создания (Сейчас)
        };

        // Хешируем пароль перед сохранением
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        // Сохраняем пользователя в базе данных
        await _userRepository.AddAsync(user);
        // Генерируем JWT-токен для нового пользователя
        // Возвращаем ответ с email и токеном
        return GenerateAuthResponse(user);
    }

    // Метод для входа пользователя
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Получаем пользователя по email из репозитория
        var user = await _userRepository.GetByEmailAsync(request.Email);
        // Проверка на null: если пользователь не найден, выбрасываем исключение
        if (user == null)
            throw new Exception("Пользователь не найден");

        // Проверяем пароль: сравниваем хешированный пароль с введенным
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        // Если проверка не удалась, выбрасываем исключение
        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Неверный пароль");
        // Если все проверки прошли успешно, генерируем JWT-токен
        return GenerateAuthResponse(user);
    }

    // Генерация JWT-токена
    private AuthResponse GenerateAuthResponse(User user)
    {
        // Создаем список утверждений (claims) для токена
        // Каждое утверждение содержит информацию о пользователе
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Создаем ключ для подписи токена и определяем алгоритм
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        // Создаем учетные данные для подписи токена
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // Устанавливаем время жизни токена (например, 2 часа)
        var expires = DateTime.UtcNow.AddHours(2);


        // Создаем JWT-токен с указанными утверждениями, сроком действия и учетными данными
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],   // Указатель (issuer) токена
            audience: _configuration["Jwt:Audience"],   // Аудитория (audience) токена
            claims: claims, // Утверждения (claims) токена
            expires: expires,   // Время истечения токена
            signingCredentials: creds   // Учетные данные для подписи токена
        );

        // Возвращаем объект AuthResponse с email пользователя и JWT-токеном
        return new AuthResponse
        {
            Email = user.Email,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
