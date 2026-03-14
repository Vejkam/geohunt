using System.Net;

namespace psi25_project.Exceptions
{
    public class GoogleMapsApiException : Exception
    {
        public string Endpoint { get; }
        public HttpStatusCode? StatusCode { get; }
        public string ErrorCode { get; }

        public GoogleMapsApiException(
            string endpoint,
            HttpStatusCode? statusCode,
            string errorCode,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Endpoint = endpoint;
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public GoogleMapsApiException(
            string endpoint,
            string errorCode,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Endpoint = endpoint;
            StatusCode = null;
            ErrorCode = errorCode;
        }
    }
}
