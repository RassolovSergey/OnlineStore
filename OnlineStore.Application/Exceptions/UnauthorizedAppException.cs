using System.Net;

namespace OnlineStore.Application.Exceptions
{
    public sealed class UnauthorizedAppException : AppException
    {
        public UnauthorizedAppException(string message = "Не авторизован") : base(message, HttpStatusCode.Unauthorized)
        {
            // Дополнительная логика, если нужна
        }
    }
}