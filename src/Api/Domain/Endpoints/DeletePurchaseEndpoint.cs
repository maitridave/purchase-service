using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class DeletePurchaseEndpoint : Endpoint<DeletePurchaseRequest>
    {
        private readonly PurchaseDbContext _context;
        
        public DeletePurchaseEndpoint(PurchaseDbContext context)
        {
            _context = context;
        }
        
        public override void Configure()
        {
            Delete("/purchase-service/api/v1/purchases/{id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Delete a purchase";
                s.Description = "Deletes a purchase record by its ID";
                s.Response(204, "Purchase deleted successfully");
                s.Response(404, "Purchase not found");
            });
        }
        
        public override async Task HandleAsync(DeletePurchaseRequest req, CancellationToken ct)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == req.Id, ct);
                
            if (purchase == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }
            
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync(ct);
            
            await SendNoContentAsync(ct);
        }
    }
}