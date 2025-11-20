// BeWarehouseHub.Core/Mappings/Profiles/CategoryMappingProfile.cs
using AutoMapper;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Category;

namespace BeWarehouseHub.Core.Mappings.Profiles;

public class CategoryMappingProfile : Profile, IMapFrom<Category>
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForMember(d => d.ProductCount, opt => opt.MapFrom(s => s.Products.Count));

        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId));
    }

    // BẮT BUỘC PHẢI CÓ DÒNG NÀY!!!
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Category, CategoryDto>();
    }
}