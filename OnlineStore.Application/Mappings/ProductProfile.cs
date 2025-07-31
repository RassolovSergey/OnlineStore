using AutoMapper;
using OnlineStore.Application.DTOs;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Mappings;

/// <summary>
/// Профиль AutoMapper для маппинга Product ↔ ProductDto.
/// </summary>
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // От Product к ProductDto
        CreateMap<Product, ProductDto>();

        // От ProductDto к Product
        CreateMap<ProductDto, Product>();
    }
}
