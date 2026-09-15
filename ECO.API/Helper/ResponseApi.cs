namespace ECO.Api.Helper
{
    public class ResponseApi
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }


        public ResponseApi(int statusCode, string? message = null) 
        {
            StatusCode = statusCode;

            var defaultMessage = statusCode switch
            {
                200 => "Success",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => "Unknown Error"
            };

            Message = message ?? defaultMessage;
        }
    }
}