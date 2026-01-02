using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AI.PurchaseService.Domain.DTOs
{
    public class CreatePurchaseRequest
    {
        [Required]
        [JsonPropertyName("buyer_id")]
        public long BuyerId { get; set; }
        
        [Required]
        [JsonPropertyName("offer_id")]
        public long OfferId { get; set; }
        
        [Required]
        [JsonPropertyName("transport_id")]
        public long TransportId { get; set; }
        
        [JsonPropertyName("assigned_at")]
        public DateTime? AssignedAt { get; set; }
        
        [JsonPropertyName("bid_amount")]
        public decimal? BidAmount { get; set; }
        
        [StringLength(20)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
    }
    
    public class UpdatePurchaseRequest
    {
        [Required]
        [JsonPropertyName("id")]
        public long Id { get; set; }
        
        [JsonPropertyName("buyer_id")]
        public long? BuyerId { get; set; }
        
        [JsonPropertyName("offer_id")]
        public long? OfferId { get; set; }
        
        [JsonPropertyName("transport_id")]
        public long? TransportId { get; set; }
        
        [JsonPropertyName("assigned_at")]
        public DateTime? AssignedAt { get; set; }
        
        [JsonPropertyName("bid_amount")]
        public decimal? BidAmount { get; set; }
        
        [StringLength(20)]
        [JsonPropertyName("status")]
        public string? Status { get; set; }

    }
    
    public class PurchaseResponse
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
        
        [JsonPropertyName("last_modified_at")]
        public DateTime LastModifiedAt { get; set; }
    }
    
    public class GetPurchaseRequest
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
    
    public class DeletePurchaseRequest
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}