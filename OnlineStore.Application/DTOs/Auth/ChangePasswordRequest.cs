namespace OnlineStore.Application.DTOs.Auth
{
    /// <summary>
    /// Запрос на смену пароля текущего пользователя.
    /// </summary>
    public class ChangePasswordRequest
    {
        // Текущий (старый) пароль — для подтверждения владения аккаунтом
        public string CurrentPassword { get; set; } = null!;

        // Новый пароль
        public string NewPassword { get; set; } = null!;
    }
}