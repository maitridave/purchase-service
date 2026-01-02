using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.DTOs;
using AI.PurchaseService.Domain.Events;
using MassTransit;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class UpdatePurchaseEndpoint : Endpoint<UpdatePurchaseRequest, PurchaseResponse>
    {
        private readonly PurchaseDbContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        
        public UpdatePurchaseEndpoint(PurchaseDbContext context, AutoMapper.IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
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
            
            // Track changes for event publishing
            var changedProperties = new Dictionary<string, object>();
            var previousValues = new Dictionary<string, object>();
            
            // Update only non-null properties and track changes
            if (req.BuyerId.HasValue && purchase.BuyerId != req.BuyerId.Value)
            {
                previousValues["buyer_id"] = purchase.BuyerId;
                purchase.BuyerId = req.BuyerId.Value;
                changedProperties["buyer_id"] = purchase.BuyerId;
            }
                
            if (req.OfferId.HasValue && purchase.OfferId != req.OfferId.Value)
            {
                previousValues["offer_id"] = purchase.OfferId;
                purchase.OfferId = req.OfferId.Value;
                changedProperties["offer_id"] = purchase.OfferId;
            }
                
            if (req.TransportId.HasValue && purchase.TransportId != req.TransportId.Value)
            {
                previousValues["transport_id"] = purchase.TransportId;
                purchase.TransportId = req.TransportId.Value;
                changedProperties["transport_id"] = purchase.TransportId;
            }
                
            if (req.AssignedAt.HasValue && purchase.AssignedAt != req.AssignedAt.Value)
            {
                previousValues["assigned_at"] = purchase.AssignedAt;
                purchase.AssignedAt = req.AssignedAt.Value;
                changedProperties["assigned_at"] = purchase.AssignedAt;
            }
                
            if (req.BidAmount.HasValue && purchase.BidAmount != req.BidAmount.Value)
            {
                previousValues["bid_amount"] = purchase.BidAmount;
                purchase.BidAmount = req.BidAmount.Value;
                changedProperties["bid_amount"] = purchase.BidAmount;
            }
                
            if (!string.IsNullOrEmpty(req.Status) && purchase.Status != req.Status)
            {
                previousValues["status"] = purchase.Status;
                purchase.Status = req.Status;
                changedProperties["status"] = purchase.Status;
            }
            
            // Only proceed if there were actual changes
            if (changedProperties.Count == 0)
            {
                var response = _mapper.Map<PurchaseResponse>(purchase);
                await SendOkAsync(response, ct);
                return;
            }
            
            purchase.LastModifiedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);
            
            // Create and publish PurchaseUpdated event
            var purchaseUpdatedEvent = new PurchaseUpdatedEvent
            {
                Id = purchase.Id,
                BuyerId = purchase.BuyerId,
                OfferId = purchase.OfferId,
                TransportId = purchase.TransportId,
                AssignedAt = purchase.AssignedAt,
                BidAmount = purchase.BidAmount,
                Status = purchase.Status,
                CreatedAt = purchase.CreatedAt,
                UpdatedAt = purchase.LastModifiedAt,
                ChangedProperties = changedProperties,
                PreviousValues = previousValues,
                EventTimestamp = DateTime.UtcNow
            };
            
            await _publishEndpoint.Publish(purchaseUpdatedEvent, ct);
            
            var responseDto = _mapper.Map<PurchaseResponse>(purchase);
            await SendOkAsync(responseDto, ct);
        }
    }
}