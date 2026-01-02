using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class UpdatePurchaseEndpoint : Endpoint<UpdatePurchaseRequest, PurchaseResponse>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        
        public UpdatePurchaseEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public override void Configure()
        {
            Put("/purchase-service/api/v1/purchases/{id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Update an existing purchase";
                s.Description = "Updates an existing purchase record";
                s.Response<PurchaseResponse>(200, "Purchase updated successfully");
                s.Response(404, "Purchase not found");
                s.Response(400, "Invalid purchase data");
            });
        }
        
        public override async Task HandleAsync(UpdatePurchaseRequest req, CancellationToken ct)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == req.Id, ct);
                
            if (purchase == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }
            
            // Update only non-null properties
            if (req.BuyerId.HasValue)
                purchase.BuyerId = req.BuyerId.Value;
                
            if (req.OfferId.HasValue)
                purchase.OfferId = req.OfferId.Value;
                
            if (req.TransportId.HasValue)
                purchase.TransportId = req.TransportId.Value;
                
            if (req.AssignedAt.HasValue)
                purchase.AssignedAt = req.AssignedAt.Value;
                
            if (req.BidAmount.HasValue)
                purchase.BidAmount = req.BidAmount.Value;
                
            if (!string.IsNullOrEmpty(req.Status))
                purchase.Status = req.Status;
            
            purchase.LastModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);
            
            var response = _mapper.Map<PurchaseResponse>(purchase);
            await SendOkAsync(response, ct);
        }
    }
}