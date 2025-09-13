namespace Askfm_Clone.DTOs.Auth
{
    public class AuthControllerResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public object? Data { get; set; }

        private AuthControllerResponseDto() { }

        public static AuthControllerResponseDto SuccessResponse(int status, string message, object? data = null)
        {
            return new AuthControllerResponseDto
            {
                StatusCode = status,
                Message = message,
                Data = data,
            };
        }

        public static AuthControllerResponseDto ErrorResponse(int status, string message, IEnumerable<string>? errors = null)
        {
            return new AuthControllerResponseDto
            {
                StatusCode = status,
                Message = message,
                Errors = errors,
            };
        }
    }
}
