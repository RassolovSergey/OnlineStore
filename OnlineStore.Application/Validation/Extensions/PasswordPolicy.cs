namespace OnlineStore.Application.Validation.Extensions
{
    public static class PasswordPolicy
    {
        public static bool HasUpper(string s) => s.Any(char.IsUpper);
        public static bool HasLower(string s) => s.Any(char.IsLower);
        public static bool HasDigit(string s) => s.Any(char.IsDigit);
        public static bool HasSpecial(string s) => s.Any(ch => !char.IsLetterOrDigit(ch));
        public static bool NoWhitespace(string s) => !s.Any(char.IsWhiteSpace);

        public static bool NotContainsEmail(string password, string email)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email)) return true;
            var trimmed = password.Trim();
            var local = email.Split('@')[0];
            return !trimmed.Contains(email, StringComparison.OrdinalIgnoreCase)
                && !trimmed.Contains(local, StringComparison.OrdinalIgnoreCase);
        }
    }
}