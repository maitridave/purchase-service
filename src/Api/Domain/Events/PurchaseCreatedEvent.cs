using System.Text.Json.Serialization;

namespace AI.PurchaseService.Domain.Events
{
    public class PurchaseCreatedEvent
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("buyer_id")]
        public long BuyerId { get; set; }
        
        [JsonPropertyName("offer_id")]
        public long OfferId { get; set; }
        
        [JsonPropertyName("transport_id")]
        public long TransportId { get; set; }
        
        [JsonPropertyName("assigned_at")]
        public DateTime? AssignedAt { get; set; }
        
        [JsonPropertyName("bid_amount")]
        public decimal? BidAmount { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("event_timestamp")]
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    }
}