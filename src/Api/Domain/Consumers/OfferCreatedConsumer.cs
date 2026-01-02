using AI.PurchaseService.Events;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.Data;
using AI.PurchaseService.Domain.Events;
using MassTransit;

namespace AI.PurchaseService.Domain.Consumers
{
    public class OfferCreatedConsumer : IConsumer<OfferCreated>
    {
        private readonly PurchaseDbContext _context;
        private readonly ILogger<OfferCreatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OfferCreatedConsumer(PurchaseDbContext context, ILogger<OfferCreatedConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OfferCreated> context)
        {
            try
            {
                var offerCreated = context.Message;
                
                if (offerCreated == null)
                {
                    _logger.LogWarning("Received null OfferCreated message");
                    return;
                }
                
                _logger.LogInformation("Received OfferCreated event for Offer ID: {OfferId}, Status: {Status}, BuyerId: {BuyerId}", 
                    offerCreated.Id, offerCreated.Status, offerCreated.BuyerId);

                // Log the raw message for debugging
                _logger.LogDebug("OfferCreated message details: {@OfferCreated}", offerCreated);

                // Only create purchase for OPEN offers
                var purchase = new Purchase
                {
                    BuyerId = offerCreated.BuyerId, // In this context, seller becomes potential buyer
                    OfferId = offerCreated.Id,
                    BidAmount = offerCreated.OfferAmount,
                    Status = "Assigned", // Initial status for new purchase 
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

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
                    CreatedAt = purchase.CreatedAt
                };

                await _publishEndpoint.Publish(purchaseCreatedEvent);

                _logger.LogInformation("Successfully created Purchase ID: {PurchaseId} from Offer ID: {OfferId} and published PurchaseCreated event", 
                    purchase.Id, offerCreated.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming OfferCreated event. Message: {Message}", ex.Message);
                
                // Log additional context information
                if (context?.Message != null)
                {
                    _logger.LogError("Failed to process OfferCreated for Offer ID: {OfferId}", context.Message.Id);
                }
                
                throw; // Re-throw to trigger retry mechanism if configured
            }
        }
    }
}