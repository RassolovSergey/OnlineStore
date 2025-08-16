using System.Net;

namespace OnlineStore.Application.Exceptions;

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "Доступ запрещён")
        : base(message, HttpStatusCode.Forbidden)
    { 
        // Дополнительная логика, если нужна
    }
}
