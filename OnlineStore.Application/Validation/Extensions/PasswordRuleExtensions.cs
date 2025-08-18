using FluentValidation;

namespace OnlineStore.Application.Validation.Extensions
{
    /// <summary>
    /// Расширения FluentValidation для единых «сильных» правил паролей.
    /// ВНИМАНИЕ: каскадирование (CascadeMode.Stop) выставляйте в валидаторе перед вызовом StrongPassword().
    /// </summary>
    public static class PasswordRuleExtensions
    {
        /// <summary>
        /// Строгие требования к паролю:
        /// - не пустой, длина 8..100;
        /// - нет пробелов в начале/конце и внутри;
        /// - есть заглавная, строчная, цифра, спецсимвол.
        /// </summary>
        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("Пароль обязателен.")
                .MinimumLength(8).WithMessage("Минимальная длина пароля — 8 символов.")
                .MaximumLength(100).WithMessage("Пароль слишком длинный.")
                .Must(p => p.Trim() == p).WithMessage("Пароль не должен начинаться/заканчиваться пробелами.")
                .Must(PasswordPolicy.NoWhitespace).WithMessage("Пароль не должен содержать пробелы.")
                .Must(PasswordPolicy.HasUpper).WithMessage("Нужна заглавная буква.")
                .Must(PasswordPolicy.HasLower).WithMessage("Нужна строчная буква.")
                .Must(PasswordPolicy.HasDigit).WithMessage("Нужна цифра.")
                .Must(PasswordPolicy.HasSpecial).WithMessage("Нужен спецсимвол.");
        }
    }
}