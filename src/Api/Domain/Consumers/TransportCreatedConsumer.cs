using AI.PurchaseService.Events;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AI.PurchaseService.Domain.Consumers
{
    public class TransportCreatedConsumer : IConsumer<TransportCreated>
    {
        private readonly PurchaseDbContext _context;
        private readonly ILogger<TransportCreatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public TransportCreatedConsumer(PurchaseDbContext context, ILogger<TransportCreatedConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<TransportCreated> context)
        {
            var transportCreated = context.Message;
            
            _logger.LogInformation("Received TransportCreated event for Transport ID: {TransportId}, OfferId: {OfferId}", 
                transportCreated.Id, transportCreated.OfferId);

            try
            {
                // Find purchase by OfferId
                var purchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.OfferId == transportCreated.OfferId);

                if (purchase == null)
                {
                    _logger.LogWarning("No purchase found with OfferId: {OfferId} for Transport ID: {TransportId}", 
                        transportCreated.OfferId, transportCreated.Id);
                    return;
                }

                // Store previous values for change tracking
                var previousTransportId = purchase.TransportId;
                var previousLastModifiedAt = purchase.LastModifiedAt;

                // Update purchase with transport information
                purchase.TransportId = transportCreated.Id;
                purchase.LastModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

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
                    ChangedProperties = new Dictionary<string, object>
                    {
                        ["transport_id"] = purchase.TransportId,
                        ["last_modified_at"] = purchase.LastModifiedAt
                    },
                    PreviousValues = new Dictionary<string, object>
                    {
                        ["transport_id"] = previousTransportId,
                        ["last_modified_at"] = previousLastModifiedAt
                    }
                };

                await _publishEndpoint.Publish(purchaseUpdatedEvent);

                _logger.LogInformation("Successfully updated Purchase ID: {PurchaseId} with Transport ID: {TransportId} and published PurchaseUpdated event", 
                    purchase.Id, transportCreated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase from TransportCreated event for Transport ID: {TransportId}, OfferId: {OfferId}", 
                    transportCreated.Id, transportCreated.OfferId);
                throw; // Re-throw to trigger retry mechanism if configured
            }
        }
    }
}