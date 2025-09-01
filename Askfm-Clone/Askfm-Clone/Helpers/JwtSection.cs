namespace Askfm_Clone.Helpers
{
    public class JwtSection
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string Key { get; set; }
        public double  AccessTokenLifetimeMinutes { get; set; }
        public double RefreshTokenLifetimeDays { get; set; }
    }
}
