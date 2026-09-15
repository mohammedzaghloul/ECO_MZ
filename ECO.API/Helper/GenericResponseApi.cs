namespace ECO.Api.Helper
{
    public class GenericResponseApi<T> : ResponseApi
    {
        public T? Data { get; set; }

        public GenericResponseApi(
            int statusCode,
            string? message = null,
            T? data = default)
            : base(statusCode, message)
        {
            Data = data;
        }
    }
}