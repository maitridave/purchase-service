using System.Text.Json.Serialization;

namespace AI.PurchaseService.Events
{
    public class OfferCreated
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("vehicle_id")]
        public long VehicleId { get; set; }
        
        [JsonPropertyName("seller_id")]
        public long SellerId { get; set; }

        [JsonPropertyName("buyer_id")]
        public long BuyerId { get; set; }
        
        [JsonPropertyName("offer_amount")]
        public decimal? OfferAmount { get; set; }
        
        [JsonPropertyName("city")]
        public string? City { get; set; }
        
        [JsonPropertyName("state")]
        public string? State { get; set; }
        
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("last_modified_at")]
        public DateTime LastModifiedAt { get; set; }
    }
}