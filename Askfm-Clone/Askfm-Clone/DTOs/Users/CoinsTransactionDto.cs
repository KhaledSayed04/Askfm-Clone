namespace Askfm_Clone.DTOs.Users
{
    public class CoinsTransactionDto
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
