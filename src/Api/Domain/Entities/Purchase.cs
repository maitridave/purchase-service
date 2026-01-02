using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI.PurchaseService.Domain.Entities
{
    public class Purchase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        [Required]
        public long BuyerId { get; set; }
        
        [Required]
        public long OfferId { get; set; }
        
        [Required]
        public long TransportId { get; set; }
        
        public DateTime? AssignedAt { get; set; }
        
        [Column(TypeName = "decimal(12,2)")]
        public decimal? BidAmount { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; } // COMPLETED, CANCELLED
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    }
    
    public static class PurchaseStatus
    {
        public const string Completed = "Completed";
        public const string Canceled = "Canceled";
        public const string Assigned = "Assigned";

    }
}