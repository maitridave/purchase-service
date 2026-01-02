using System.Text.Json.Serialization;

namespace AI.PurchaseService.Domain.Events
{
    public class PurchaseUpdatedEvent
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
        
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonPropertyName("changed_properties")]
        public Dictionary<string, object> ChangedProperties { get; set; } = new();
        
        [JsonPropertyName("previous_values")]
        public Dictionary<string, object> PreviousValues { get; set; } = new();
        
        [JsonPropertyName("event_timestamp")]
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    }
}