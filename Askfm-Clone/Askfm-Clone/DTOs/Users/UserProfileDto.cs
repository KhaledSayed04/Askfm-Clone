namespace Askfm_Clone.DTOs.Users
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool AllowAnonymous { get; set; }
    }
}
