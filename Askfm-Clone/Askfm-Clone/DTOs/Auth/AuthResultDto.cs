namespace Askfm_Clone.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool successFlag { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
        public int? Status { get; set; }


        public static AuthResultDto Success(string message, object? data = null, int? status = null) => new AuthResultDto
        {
            successFlag = true,
            Message = message,
            Data = data,
        };

        public static AuthResultDto Fail(string message) => new AuthResultDto
        {
            successFlag = false,
            Message = message,
        };

    }
}
