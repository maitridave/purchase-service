using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.Events;
using MassTransit;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class CreatePurchaseEndpoint : Endpoint<CreatePurchaseRequest, PurchaseResponse>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        
        public CreatePurchaseEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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
            
            // Publish PurchaseCreated event to RabbitMQ
            var purchaseCreatedEvent = new PurchaseCreatedEvent
            {
                Id = purchase.Id,
                BuyerId = purchase.BuyerId,
                OfferId = purchase.OfferId,
                TransportId = purchase.TransportId,
                AssignedAt = purchase.AssignedAt,
                BidAmount = purchase.BidAmount,
                Status = purchase.Status,
                BuyerName = purchase.BuyerName,
                CreatedAt = purchase.LastModifiedAt
            };
            
            await _publishEndpoint.Publish(purchaseCreatedEvent, ct);
            
            var response = _mapper.Map<PurchaseResponse>(purchase);
            await SendCreatedAtAsync<GetPurchaseEndpoint>(new { Id = purchase.Id }, response, cancellation: ct);
        }
    }
}