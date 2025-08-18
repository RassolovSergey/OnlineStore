using FluentValidation;

namespace OnlineStore.Application.Validation.Extensions
{
    // Проверяет, что email содержит доменную зону с TLD (типа .com, .ru)
    public static class EmailRuleExtensions
    {
        public static IRuleBuilderOptions<T, string> MustHaveDomainWithTld<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.Must(email =>
            {
                if (string.IsNullOrWhiteSpace(email)) return false;
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email.Trim());
                    var host = addr.Host;
                    var lastDot = host.LastIndexOf('.');
                    if (lastDot <= 0 || lastDot == host.Length - 1) return false;
                    var tld = host[(lastDot + 1)..];
                    return tld.Length >= 2 && tld.Length <= 63;
                }
                catch { return false; }
            })
            .WithMessage("Email должен содержать доменную зону (например, .com).");
        }
    }
}