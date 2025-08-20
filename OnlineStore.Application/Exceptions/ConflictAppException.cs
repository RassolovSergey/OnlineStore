// Исключение уровня приложения для конфликтов (HTTP 409 Conflict)
// Сценарии: дубликат email при регистрации, конфликт версий и т.п.

using System.Net;

namespace OnlineStore.Application.Exceptions
{
    // Класс обработчик ошибок
    public sealed class ConflictAppException : AppException
    {
        public ConflictAppException(string message) : base(message, HttpStatusCode.Conflict) { }
    }

}
