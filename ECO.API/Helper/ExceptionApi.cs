namespace ECO.Api.Helper
{
    public class ExceptionApi : ResponseApi
    {
        public ExceptionApi(int statusCode, string? message = null, string? details = null) : base(statusCode, message)
        {
            Details = details;
        }
        public string? Details { get; set; }
    }
}
