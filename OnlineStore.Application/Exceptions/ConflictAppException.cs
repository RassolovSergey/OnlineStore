// Исключение уровня приложения для конфликтов (HTTP 409 Conflict)
// Сценарии: дубликат email при регистрации, конфликт версий и т.п.

namespace OnlineStore.Application.Exceptions
{
    // Класс обработчик ошибок
    public sealed class ConflictAppException : Exception
    {
        public ConflictAppException(string message) : base(message) { }
        public ConflictAppException(string message, Exception? inner) : base(message, inner) { }
    }
}
