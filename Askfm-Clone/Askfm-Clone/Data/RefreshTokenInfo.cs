namespace Askfm_Clone.Data
{
   
    public class RefreshTokenInfo
    {
     public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public bool Revoked { get; set; } = false;

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    }
    
}
