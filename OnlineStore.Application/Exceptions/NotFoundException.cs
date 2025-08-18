using System.Net;

namespace OnlineStore.Application.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
        {
            // Дополнительная логика, если нужна
        }
    }
}