using System.Net;

namespace OnlineStore.Application.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, HttpStatusCode.Conflict)
    { 
        // Дополнительная логика, если нужна
    }
}
