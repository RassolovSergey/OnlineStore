using FluentValidation;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Extensions;

namespace OnlineStore.Application.Validation.Auth
{
    /// <summary>
    /// Правила валидации смены пароля.
    /// </summary>
    public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Текущий пароль обязателен.")
                .MaximumLength(256);

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .StrongPassword(); // те же строгие требования, что и при регистрации

            RuleFor(x => x)
                .Must(x => x.CurrentPassword != x.NewPassword)
                .WithMessage("Новый пароль не должен совпадать с текущим.");
        }
    }
}