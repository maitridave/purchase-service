using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AI.PurchaseService.Domain.Entities;

namespace AI.PurchaseService.Domain.Configuration
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.ToTable("Purchase");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd(); // Auto-generate Id on insert
                
            builder.Property(p => p.BuyerId)
                .IsRequired();
                
            builder.Property(p => p.OfferId)
                .IsRequired();
                
            builder.Property(p => p.TransportId)
                .IsRequired();
                
            builder.Property(p => p.AssignedAt)
                .HasColumnType("datetime2");
                
            builder.Property(p => p.BidAmount)
                .HasColumnType("decimal(12,2)");
                
            builder.Property(p => p.Status)
                .HasMaxLength(20);
                
            builder.Property(p => p.BuyerName)
                .HasMaxLength(100);
                
            builder.Property(p => p.LastModifiedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}