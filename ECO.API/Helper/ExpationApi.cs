namespace ECO.Api.Helper
{
    public class ExcepationsApi : ResponseApi
    {
        public ExcepationsApi(int statusCode, string? message = null, string? details = null) : base(statusCode, message)
        {
            Details = details;
        }
        public string Details { get; set; }
    }
}
