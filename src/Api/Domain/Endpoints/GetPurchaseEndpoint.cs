using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class GetPurchaseEndpoint : Endpoint<GetPurchaseRequest, PurchaseResponse>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        
        public GetPurchaseEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public override void Configure()
        {
            Get("/purchase-service/api/v1/purchases/{id}");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get purchase by ID";
                s.Description = "Retrieves a purchase record by its ID";
                s.Response<PurchaseResponse>(200, "Purchase found");
                s.Response(404, "Purchase not found");
            });
        }
        
        public override async Task HandleAsync(GetPurchaseRequest req, CancellationToken ct)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == req.Id, ct);
                
            if (purchase == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }
            
            var response = _mapper.Map<PurchaseResponse>(purchase);
            await SendOkAsync(response, ct);
        }
    }
}