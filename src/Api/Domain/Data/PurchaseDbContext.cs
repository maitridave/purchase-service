using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.Configuration;

namespace AI.PurchaseService.Domain.Data
{
    public class PurchaseDbContext : DbContext
    {
        public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options) : base(options)
        {
        }
        
        public DbSet<Purchase> Purchases { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        }
        
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<Purchase>()
                .Where(e => e.State == EntityState.Modified);
                
            foreach (var entry in entries)
            {
                entry.Entity.LastModifiedAt = DateTime.UtcNow;
            }
        }
    }
}