using System.Net;

namespace OnlineStore.Application.Exceptions
{
    /// <summary>
    /// Базовое прикладное исключение с HTTP-статусом (без зависимости от ASP.NET).
    /// </summary>
    public abstract class AppException : Exception
    {
        // Храним стандартный статус из BCL
        public HttpStatusCode StatusCode { get; }

        protected AppException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}