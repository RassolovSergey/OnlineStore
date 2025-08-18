using FluentValidation;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Extensions;

namespace OnlineStore.Application.Validation.Auth
{
    /// <summary>
    /// Правила валидации для регистрации пользователя.
    /// </summary>
    public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email обязателен.")
                .MinimumLength(5).WithMessage("Минимальная длина email — 5 символов.")
                .MaximumLength(255).WithMessage("Email слишком длинный.")
                .EmailAddress().WithMessage("Неверный формат email.")
                .MustHaveDomainWithTld();

            // Каскадирование ставим снаружи перед StrongPassword()
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .StrongPassword();

            // Пароль не должен содержать email/локальную часть
            RuleFor(x => x)
                .Must(x => PasswordPolicy.NotContainsEmail(x.Password, x.Email))
                .WithMessage("Пароль не должен содержать ваш email.");
        }
    }
}