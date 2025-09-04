namespace Askfm_Clone.DTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }

}
