using FluentValidation;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Extensions;

namespace OnlineStore.Application.Validation.Auth
{
    /// <summary>
    /// Правила валидации для логина (минимальные, без подсказок по сложности).
    /// </summary>
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email обязателен.")
                .EmailAddress().WithMessage("Некорректный формат email.")
                .MustHaveDomainWithTld()
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен.")
                .MaximumLength(256);
        }
    }
}