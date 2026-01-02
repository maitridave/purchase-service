using FastEndpoints;
using FluentValidation;
using AI.PurchaseService.Domain.DTOs;
using AI.PurchaseService.Domain.Entities;

namespace AI.PurchaseService.Domain.Validators
{
    public class CreatePurchaseValidator : Validator<CreatePurchaseRequest>
    {
        public CreatePurchaseValidator()
        {
            RuleFor(x => x.BuyerId)
                .NotEmpty()
                .WithMessage("Buyer ID is required");

            RuleFor(x => x.OfferId)
                .NotEmpty()
                .WithMessage("Offer ID is required");

            RuleFor(x => x.TransportId)
                .NotEmpty()
                .WithMessage("Transport ID is required");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || 
                               status == PurchaseStatus.Completed || 
                               status == PurchaseStatus.Assigned || 
                               status == PurchaseStatus.Canceled 
                               )
                .WithMessage("Status must be either 'COMPLETED' or 'CANCELLED'");

            RuleFor(x => x.BidAmount)
                .GreaterThan(0)
                .When(x => x.BidAmount.HasValue)
                .WithMessage("Bid amount must be greater than 0");
        }
    }

    public class UpdatePurchaseValidator : Validator<UpdatePurchaseRequest>
    {
        public UpdatePurchaseValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Purchase ID is required");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || 
                               status == PurchaseStatus.Completed || 
                               status == PurchaseStatus.Assigned || 

                               status == PurchaseStatus.Canceled)
                .WithMessage("Status must be either 'COMPLETED' or 'CANCELLED'");

            RuleFor(x => x.BidAmount)
                .GreaterThan(0)
                .When(x => x.BidAmount.HasValue)
                .WithMessage("Bid amount must be greater than 0");
        }
    }
}