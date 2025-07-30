namespace Domain.DTOs.Subscriptions
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Query { get; set; } = default!;
    }
}
