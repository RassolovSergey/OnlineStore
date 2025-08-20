using OnlineStore.Application.DTOs.Auth;

namespace OnlineStore.Application.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса аутентификации и регистрации пользователей.
    /// </summary>
    public interface IAuthService
    {
        // Регистрация нового пользователя, обрабатывает запрос на регистрацию
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        // Вход по email и паролю, обрабатывает запрос на вход
        Task<AuthResponse> LoginAsync(LoginRequest request);

        // Получение профиля пользователя по его идентификатору
        // Возвращает профиль пользователя, если он существует
        Task<UserProfileDto> GetProfileAsync(Guid userId);

        // Изменение пароля текущего пользователя
        // Принимает идентификатор пользователя и новый пароль, обновляет пароль в системе
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);

        Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip = null, string? ua = null);
        Task LogoutAsync(string refreshToken, string? ip = null);
        Task<IReadOnlyList<SessionDto>> GetSessionsAsync(Guid userId, CancellationToken ct = default);
        Task LogoutAllAsync(Guid userId, string? ip = null, CancellationToken ct = default);
        Task LogoutSessionAsync(Guid userId, Guid tokenId, string? ip = null, CancellationToken ct = default);

    }
}