using System.Net.Mail;
using FluentValidation;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Extensions;

namespace OnlineStore.Application.Validation.Auth;

/// <summary>
/// Правила валидации для регистрации.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    // Конструктор, в котором определяются правила валидации
    // Используем FluentValidation для определения правил
    public RegisterRequestValidator()
    {
        // Правила для полей Email и Password
        // Используем FluentValidation для определения правил валидации
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен.")    // Проверка на пустоту
            .MinimumLength(5).WithMessage("Минимальная длина email — 5 символов.") // Минимальная длина email
            .MaximumLength(255).WithMessage("Email слишком длинный.")   // Максимальная длина email
            .EmailAddress().WithMessage("Неверный формат email.")  // Проверка на корректный формат email
            .MustHaveDomainWithTld();


        // Проверка на уникальность email должна быть реализована в сервисе, а не в валидаторе
        // RuleFor(x => x.Email).MustAsync(async (email, cancellation) =>
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен.")   // Проверка на пустоту
            .MinimumLength(6).WithMessage("Минимальная длина пароля — 6 символов.") // Минимальная длина пароля
            .MaximumLength(100).WithMessage("Пароль слишком длинный.")  // Максимальная длина пароля
            .Must(p => p.Trim() == p).WithMessage("Пароль не должен начинаться/заканчиваться пробелами.");   // Проверка на пробелы в начале и конце пароля
    }
}
