using AutoMapper;
using ECommerce.Domain.Products.Contracts.Requests;
using ECommerce.Domain.Products.Entity;

namespace ECommerce.Domain.Products.Config;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<CreateProductRequest, Product>()
            .ForMember(e => e.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(e => e.Id, opt => opt.Ignore())
            .ForMember(e => e.Sku, opt => opt.Ignore());
    }
}
