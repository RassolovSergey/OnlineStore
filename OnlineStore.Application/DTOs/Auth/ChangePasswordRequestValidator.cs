using FluentValidation;
using OnlineStore.Application.DTOs.Auth;

namespace OnlineStore.Application.Validation.Auth;

/// <summary>
/// Правила валидации смены пароля.
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    // Конструктор, определяющий правила валидации
    public ChangePasswordRequestValidator()
    {
        // Правила для текущего и нового пароля
        RuleFor(x => x.CurrentPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Текущий пароль обязателен.");

        // Новый пароль должен быть не пустым, иметь минимальную длину и не содержать пробелов в начале/конце
        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Новый пароль обязателен.")
            .MinimumLength(6).WithMessage("Минимальная длина пароля — 6 символов.")
            .Must(p => p.Trim() == p).WithMessage("Пароль не должен начинаться/заканчиваться пробелами.");

        // Новый пароль не должен совпадать со старым
        RuleFor(x => x)
            .Must(x => x.CurrentPassword != x.NewPassword)
            .WithMessage("Новый пароль не должен совпадать с текущим.");
    }
}
