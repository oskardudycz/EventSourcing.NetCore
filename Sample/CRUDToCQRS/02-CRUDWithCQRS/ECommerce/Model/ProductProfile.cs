using AutoMapper;
using ECommerce.Requests;

namespace ECommerce.Model;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<CreateProductRequest, Product>()
            .ForMember(e => e.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(e => e.Sku, opt => opt.Ignore());
    }
}
