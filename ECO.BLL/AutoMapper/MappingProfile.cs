using AutoMapper;
using ECO.BLL.DTO.AddressDtos;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.DTO.Auth;
using ECO.BLL.DTO.CategoryDtos;
using ECO.BLL.DTO.Order;
using ECO.BLL.DTO.OrderDtos;
using ECO.BLL.DTO.ProductDtos;
using ECO.BLL.DTO.ReviewDtos;
using ECO.DAL.Entities;
using ECO.DAL.Entities.Landing;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Entities.Product;

namespace ECO.BLL.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, RegisterDto>().ReverseMap();
            CreateMap<ShippingAddressDto, ShippingAddress>();
            CreateMap<ShippingAddressDto, AddressAddDto>();
            CreateMap<Order, OrderDto>()
                .ForMember(destination => destination.DeliveryMethodId,
                    options => options.MapFrom(source => source.DeliveryMethod.Id))
                .ForMember(destination => destination.ShippingAddressDto,
                    options => options.MapFrom(source => source.ShippingAddress));
            CreateMap<ShippingAddress, ShippingAddressDto>();
            CreateMap<AddressAddDto, Address>();
            CreateMap<Address, AddressDto>();
            CreateMap<DeliveryMethod, DeliveryMethodDto>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Order, OrderToReturnDto>()
                .ForMember(destination => destination.Status,
                    options => options.MapFrom(source => source.Status.ToString()))
                .ForMember(destination => destination.Total,
                    options => options.MapFrom(source => source.GetTotal()))
                .ForMember(destination => destination.Discount,
                    options => options.MapFrom(source => source.Discount))
                .ForMember(destination => destination.ShippingPrice,
                    options => options.MapFrom(source => source.ShippingPrice))
                .ForMember(destination => destination.DeliveryMethodId,
                    options => options.MapFrom(source => source.DeliveryMethod.Id))
                .ForMember(destination => destination.DeliveryMethodName,
                    options => options.MapFrom(source => source.DeliveryMethod.Name))
                .ForMember(destination => destination.ShippingAddress,
                    options => options.MapFrom(source => source.ShippingAddress));
            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<Category, UpdateCategoryDto>().ReverseMap();

            CreateMap<Category, AddCategoryDto>().ReverseMap();
            CreateMap<ProductSpecItem, ProductSpecificationDto>();

            // Product -> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos.Select(p => p.Name).ToList()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Count == 0 ? 0 : Math.Round(src.Reviews.Average(review => (decimal)review.Rating), 1)))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Specifications.OrderBy(s => s.SortOrder)));


            // Product -> AddProductDto
            CreateMap<Product, AddProductDto>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // AddProductDto -> Product
            CreateMap<AddProductDto, Product>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore());

            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore()).ReverseMap();

            // Review -> ReviewDto
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName,
                    options => options.MapFrom(source =>
                        source.User != null ? (source.User.DisplayName ?? source.User.UserName) : string.Empty))
                .ForMember(dest => dest.Verified,
                    options => options.MapFrom(source => source.IsVerifiedPurchase));

            CreateMap<AddReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();

            // Discounts
            CreateMap<Discount, DiscountDto>();
            CreateMap<SaveDiscountDto, Discount>();
        }
    }
}
