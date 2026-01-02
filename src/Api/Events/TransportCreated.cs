using System.Text.Json.Serialization;

namespace AI.PurchaseService.Events
{
    public class TransportCreated
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("offer_id")]
        public long OfferId { get; set; }
        
        [JsonPropertyName("purchase_id")]
        public long PurchaseId { get; set; }
        
        [JsonPropertyName("carrier_id")]
        public long CarrierId { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("carrier_name")]
        public string? CarrierName { get; set; }
        
        [JsonPropertyName("pickup_city")]
        public string? PickupCity { get; set; }
        
        [JsonPropertyName("pickup_state")]
        public string? PickupState { get; set; }
        
        [JsonPropertyName("pickup_zip_code")]
        public string? PickupZipCode { get; set; }
        
        [JsonPropertyName("dropoff_city")]
        public string? DropoffCity { get; set; }
        
        [JsonPropertyName("dropoff_state")]
        public string? DropoffState { get; set; }
        
        [JsonPropertyName("dropoff_zip_code")]
        public string? DropoffZipCode { get; set; }
        
        [JsonPropertyName("scheduled")]
        public DateTime? Scheduled { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}