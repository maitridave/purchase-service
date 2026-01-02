using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class GetAllPurchasesEndpoint : EndpointWithoutRequest<List<PurchaseResponse>>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        
        public GetAllPurchasesEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public override void Configure()
        {
            Get("/purchase-service/api/v1/purchases");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all purchases";
                s.Description = "Retrieves all purchase records";
                s.Response<List<PurchaseResponse>>(200, "List of all purchases");
            });
        }
        
        public override async Task HandleAsync(CancellationToken ct)
        {
            var purchases = await _context.Purchases
                .OrderByDescending(p => p.LastModifiedAt)
                .ToListAsync(ct);
                
            var response = _mapper.Map<List<PurchaseResponse>>(purchases);
            await SendOkAsync(response, ct);
        }
    }
}