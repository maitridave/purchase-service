using AutoMapper;
using AI.PurchaseService.Domain.Entities;
using AI.PurchaseService.Domain.DTOs;

namespace AI.PurchaseService.Domain.Mappings
{
    public class PurchaseMappingProfile : Profile
    {
        public PurchaseMappingProfile()
        {
            CreateMap<CreatePurchaseRequest, Purchase>();
            CreateMap<UpdatePurchaseRequest, Purchase>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Purchase, PurchaseResponse>();
        }
    }
}