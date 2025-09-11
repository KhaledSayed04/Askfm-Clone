namespace Askfm_Clone.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool successFlag { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
        public int? Status { get; set; }


        /// <summary>
        /// Creates an <see cref="AuthResultDto"/> representing a successful operation.
        /// </summary>
        /// <param name="message">A human-readable success message to store in the result.</param>
        /// <param name="data">Optional payload to include with the result.</param>
        /// <param name="status">An optional status value; currently accepted but not stored on the returned result.</param>
        /// <returns>An <see cref="AuthResultDto"/> with <c>successFlag</c> set to <c>true</c>, <c>Message</c> set to <paramref name="message"/>, and <c>Data</c> set to <paramref name="data"/>.</returns>
        public static AuthResultDto Success(string message, object? data = null, int? status = null) => new AuthResultDto
        {
            successFlag = true,
            Message = message,
            Data = data,
        };

        /// <summary>
        /// Creates an AuthResultDto representing a failed operation.
        /// </summary>
        /// <param name="message">A message describing the failure (typically user-facing or diagnostic).</param>
        /// <returns>An AuthResultDto with <c>successFlag</c> set to <c>false</c> and <c>Message</c> set to <paramref name="message"/>. <c>Data</c> and <c>Status</c> remain unset.</returns>
        public static AuthResultDto Fail(string message) => new AuthResultDto
        {
            successFlag = false,
            Message = message,
        };

    }
}
