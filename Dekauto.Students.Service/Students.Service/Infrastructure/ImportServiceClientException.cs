namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    /// <summary>
    /// Ошибка вызова сервиса импорта с известным HTTP-кодом и телом ответа.
    /// </summary>
    public sealed class ImportServiceClientException : Exception
    {
        public int StatusCode { get; }
        public string? ResponseBody { get; }

        public ImportServiceClientException(int statusCode, string? responseBody, string? message = null)
            : base(message ?? $"Сервис импорта вернул код {statusCode}.")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
