using FluentValidation;

namespace OnlineStore.Application.Validation.Extensions;

// Расширение для FluentValidation, которое добавляет правило проверки email на наличие доменной зоны с TLD
public static class EmailRuleExtensions
{
    // Метод расширения для IRuleBuilder, который проверяет наличие доменной зоны с TLD в email
    // Возвращает IRuleBuilderOptions с кастомным правилом проверки
    public static IRuleBuilderOptions<T, string> MustHaveDomainWithTld<T>(this IRuleBuilder<T, string> rule)
    {
        // Используем метод Must для добавления кастомной проверки
        return rule.Must(email =>
        {
            // Проверяем, что email не пустой и содержит доменную зону с TLD
            // Если email пустой, возвращаем false
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email.Trim());   // Создаем объект MailAddress для проверки email
                var host = addr.Host;   // Получаем доменную часть после @
                var lastDot = host.LastIndexOf('.');    // Находим последнюю точку в доменной части
                if (lastDot <= 0 || lastDot == host.Length - 1) return false;   // Если нет точки или точка в конце, возвращаем false
                var tld = host[(lastDot + 1)..];    // Получаем TLD (часть после последней точки)
                return tld.Length >= 2 && tld.Length <= 63; // Проверяем, что TLD имеет длину от 2 до 63 символов
                // Если все проверки пройдены, возвращаем true
            }
            // Если возникла ошибка при создании MailAddress (например, неправильный формат email), возвращаем false
            catch { return false; }
        })
        // Добавляем сообщение об ошибке, если проверка не прошла
        .WithMessage("Email должен содержать доменную зону (например, .com).");
    }
}
