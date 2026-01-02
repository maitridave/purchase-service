using AI.PurchaseService.Events;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AI.PurchaseService.Domain.Consumers
{
    public class OfferUpdatedConsumer : IConsumer<OfferUpdated>
    {
        private readonly PurchaseDbContext _context;
        private readonly ILogger<OfferUpdatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OfferUpdatedConsumer(PurchaseDbContext context, ILogger<OfferUpdatedConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OfferUpdated> context)
        {
            var offerUpdated = context.Message;
            
            _logger.LogInformation("Received OfferUpdated event for Offer ID: {OfferId}", offerUpdated.Id);

            try
            {
                // Find purchase by OfferId
                var purchase = await _context.Purchases
                    .FirstOrDefaultAsync(p => p.OfferId == offerUpdated.Id);

                if (purchase == null)
                {
                    _logger.LogInformation("No purchase found with OfferId: {OfferId} for OfferUpdated event", offerUpdated.Id);
                    return;
                }

                // Store previous values for change tracking
                var previousBuyerId = purchase.BuyerId;
                var previousBidAmount = purchase.BidAmount;
                var previousStatus = purchase.Status;
                var previousLastModifiedAt = purchase.LastModifiedAt;

                var changedProperties = new Dictionary<string, object>();
                var previousValues = new Dictionary<string, object>();

                // Update purchase fields based on offer changes
                if (offerUpdated.BuyerId.HasValue && purchase.BuyerId != offerUpdated.BuyerId.Value)
                {
                    previousValues["buyer_id"] = purchase.BuyerId;
                    purchase.BuyerId = offerUpdated.BuyerId.Value;
                    changedProperties["buyer_id"] = purchase.BuyerId;
                }

                if (purchase.BidAmount != offerUpdated.OfferAmount)
                {
                    previousValues["bid_amount"] = purchase.BidAmount;
                    purchase.BidAmount = offerUpdated.OfferAmount;
                    changedProperties["bid_amount"] = purchase.BidAmount;
                }

                // Map offer status to purchase status if needed
                var newPurchaseStatus = MapOfferStatusToPurchaseStatus(offerUpdated.Status);
                if (purchase.Status != newPurchaseStatus)
                {
                    previousValues["status"] = purchase.Status;
                    purchase.Status = newPurchaseStatus;
                    changedProperties["status"] = purchase.Status;
                }

                // Always update LastModifiedAt
                previousValues["last_modified_at"] = purchase.LastModifiedAt;
                purchase.LastModifiedAt = DateTime.UtcNow;
                changedProperties["last_modified_at"] = purchase.LastModifiedAt;

                if (changedProperties.Count > 1) // More than just LastModifiedAt changed
                {
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
                        ChangedProperties = changedProperties,
                        PreviousValues = previousValues
                    };

                    await _publishEndpoint.Publish(purchaseUpdatedEvent);

                    _logger.LogInformation("Successfully updated Purchase ID: {PurchaseId} from OfferUpdated event and published PurchaseUpdated event. Changed properties: {ChangedProperties}", 
                        purchase.Id, string.Join(", ", changedProperties.Keys.Where(k => k != "last_modified_at")));
                }
                else
                {
                    _logger.LogInformation("No significant changes detected for Purchase ID: {PurchaseId} from OfferUpdated event", purchase.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase from OfferUpdated event for Offer ID: {OfferId}", offerUpdated.Id);
                throw; // Re-throw to trigger retry mechanism if configured
            }
        }

        private string? MapOfferStatusToPurchaseStatus(string? offerStatus)
        {
            return offerStatus?.ToUpper() switch
            {
                "Assigned" => "Assigned",
                "Completed" => "Completed",
                "Canceled" => "Canceled",
                _ => "Assigned" // Default status
            };
        }
    }
}