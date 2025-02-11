namespace PriceTracking.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string AdUrl { get; set; }
        public string Email { get; set; }
        public decimal? PreviousPrice { get; set; }
    }
}
