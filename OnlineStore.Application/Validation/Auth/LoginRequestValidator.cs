using System.Net.Mail;
using FluentValidation;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Extensions;

namespace OnlineStore.Application.Validation.Auth;

/// <summary>
/// Правила валидации для логина.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    // Конструктор, в котором определяются правила валидации
    // Используем FluentValidation для определения правил
    public LoginRequestValidator()
    {
        // Правила для полей Email и Password
        // Используем FluentValidation для определения правил валидации
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email обязателен.")    // Проверка на пустоту
            .MinimumLength(5).WithMessage("Минимальная длина email — 5 символов.")  // Минимальная длина email
            .MaximumLength(255).WithMessage("Email слишком длинный.")   // Максимальная длина email
            .MustHaveDomainWithTld()
            .EmailAddress().WithMessage("Неверный формат email.");   // Проверка на корректный формат email

        // Проверка на наличие пароля
        // Пароль не должен быть пустым, но другие проверки (длина, пробелы) могут быть опциональными
        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .Must(p => p.Trim() == p).WithMessage("Пароль не должен начинаться/заканчиваться пробелами.")
            .MinimumLength(6).WithMessage("Минимальная длина пароля — 6 символов.") // Минимальная длина пароля
            .MaximumLength(100).WithMessage("Пароль слишком длинный.")  // Максимальная длина пароля
            .Matches(@"^[^\s]+$").WithMessage("Пароль не должен содержать пробелы.") // Проверка на пробелы в пароле
            .Matches(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;\""'<>?,./\\-]+$").WithMessage("Пароль содержит недопустимые символы."); // Проверка на недопустимые символы
    }
}
