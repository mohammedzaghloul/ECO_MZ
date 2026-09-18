using AutoMapper;
using ECO.BLL.DTO;
using ECO.BLL.DTO.CategoryDtos;
using ECO.DAL.Entites.Product;

namespace ECO.BLL.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<Category, UpdateCategoryDto>().ReverseMap();

            CreateMap<Category, AddCategoryDto>().ReverseMap();

            // Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos.Select(p => p.Name).ToList()))
                .ReverseMap();


            // Product -> AddProductDto
            CreateMap<Product, AddProductDto>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // AddProductDto -> Product
            CreateMap<AddProductDto, Product>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore()).ReverseMap();
        }
    }
}