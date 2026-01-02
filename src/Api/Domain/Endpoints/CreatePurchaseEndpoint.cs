using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;
using AI.PurchaseService.Domain.Entities;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class CreatePurchaseEndpoint : Endpoint<CreatePurchaseRequest, PurchaseResponse>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        
        public CreatePurchaseEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public override void Configure()
        {
            Post("/purchase-service/api/v1/purchases");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Create a new purchase";
                s.Description = "Creates a new purchase record in the system with auto-generated ID";
                s.Response<PurchaseResponse>(201, "Purchase created successfully");
                s.Response(400, "Invalid purchase data");
            });
        }
        
        public override async Task HandleAsync(CreatePurchaseRequest req, CancellationToken ct)
        {
            var purchase = _mapper.Map<Purchase>(req);
            purchase.LastModifiedAt = DateTime.UtcNow;
            
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync(ct);
            
            var response = _mapper.Map<PurchaseResponse>(purchase);
            await SendCreatedAtAsync<GetPurchaseEndpoint>(new { Id = purchase.Id }, response, cancellation: ct);
        }
    }
}