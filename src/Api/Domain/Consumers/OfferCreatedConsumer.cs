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
            var offerCreated = context.Message;
            
            _logger.LogInformation("Received OfferCreated event for Offer ID: {OfferId}", offerCreated.Id);

            try
            {
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
                _logger.LogError(ex, "Error creating purchase from OfferCreated event for Offer ID: {OfferId}", 
                    offerCreated.Id);
                throw; // Re-throw to trigger retry mechanism if configured
            }
        }
    }
}