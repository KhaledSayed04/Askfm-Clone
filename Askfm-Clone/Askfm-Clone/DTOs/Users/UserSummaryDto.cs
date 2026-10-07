namespace Askfm_Clone.DTOs.Users
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
    }
}
