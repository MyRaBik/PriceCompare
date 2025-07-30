namespace Domain.DTOs.Subscriptions
{
    public class AdminSubscriptionGroupDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = default!;
        public List<SubscriptionDto> Subscriptions { get; set; } = new();
    }
}
