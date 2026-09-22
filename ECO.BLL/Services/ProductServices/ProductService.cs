using AutoMapper;
using ECO.BLL.Services.LandingSer;
using ECO.DAL.Sharing;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using System.Linq.Expressions;
using ECO.BLL.Services.Upload;
using System.Text.Json;
using ECO.BLL.DTO.ProductDtos;

namespace ECO.BLL.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageManagementService _imageService;
        private readonly IMapper _mapper;
        private readonly ILandingService _landingService;

        public ProductService(
            IUnitOfWork unitOfWork, 
            IImageManagementService imageService, 
            IMapper mapper,
            ILandingService landingService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _mapper = mapper;
            _landingService = landingService;
        }

        public async Task<ProductDto> AddAsync(AddProductDto dto, CancellationToken cancellationToken = default)
        {
            var product = _mapper.Map<Product>(dto);
            product.Specifications = ParseSpecifications(dto.Specifications);
            await _unitOfWork.ProductRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            if (dto.Photos != null && dto.Photos.Count > 0)
            {
                var imageUrls = await _imageService.AddImageAsync(dto.Photos, "Products");

                foreach (var imageUrl in imageUrls)
                {
                    var photo = new Photo
                    { 
                        Name = imageUrl,
                        ProductId = product.Id
                    };
                    await _unitOfWork.Repository<Photo>().AddAsync(photo, cancellationToken);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            var savedProduct = await _unitOfWork.ProductRepository
                .FirstOrDefaultAsync(new ProductSpecification(product.Id), cancellationToken);

            var productDto = _mapper.Map<ProductDto>(savedProduct);

            try
            {
                var mainImage = savedProduct?.Photos?.FirstOrDefault()?.Name;
                var generateDefaultForProductDto = new ECO.BLL.DTO.LandingDtos.GenerateDefaultForProductDto
                {
                    productId = product.Id,
                    productName = product.Name,
                    description = product.Description,
                    price = product.NewPrice,
                    mainImageUrl = mainImage
                };
                await _landingService.GenerateDefaultForProductAsync(generateDefaultForProductDto, cancellationToken);
            }
            catch
            {
                // Non-blocking: failure to generate landing page does not fail product creation
            }

            return productDto;
        }
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(Expression<Func<Product, object>>[] includes, CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.ProductRepository.ListAsync(new ProductSpecification(includes), cancellationToken);
            return _mapper.Map<IReadOnlyList<ProductDto>>(products);
        }
        public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.ProductRepository.FirstOrDefaultAsync(new ProductSpecification(id), cancellationToken);
            return _mapper.Map<ProductDto?>(product);
        }
        public async Task<ProductDto?> GetByIdAsync(int id, Expression<Func<Product, object>>[] includes, CancellationToken cancellationToken = default)
        {
            var specification = new ProductSpecification(id).WithIncludes(includes);
            var product = await _unitOfWork.ProductRepository.FirstOrDefaultAsync(specification, cancellationToken);
            return _mapper.Map<ProductDto?>(product);
        }
        public async Task<bool> UpdateAsync(UpdateProductDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.ProductRepository.FirstOrDefaultAsync(
                new ProductSpecification(dto.Id).WithIncludes(new Expression<Func<Product, object>>[] { p => p.Photos, p => p.Specifications }),
                cancellationToken);
            if (product == null)
                return false;

            _mapper.Map(dto, product);
            product.Specifications.Clear();
            product.Specifications.AddRange(ParseSpecifications(dto.Specifications));

            if (dto.Photos != null && dto.Photos.Count > 0)
            {
               
                    foreach (var oldPhoto in product.Photos)
                    {
                        await _imageService.DeleteImageAsync(oldPhoto.Name);
                        await _unitOfWork.Repository<Photo>().DeleteAsync(oldPhoto.Id, cancellationToken);
                    }
                

                var newImageUrls = await _imageService.AddImageAsync(dto.Photos, "Products");
                foreach (var imageUrl in newImageUrls)
                {
                    var photo = new Photo
                    {
                        Name = imageUrl,
                        ProductId = product.Id
                    };
                    await _unitOfWork.Repository<Photo>().AddAsync(photo, cancellationToken);
                }
            }

            await _unitOfWork.ProductRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        private static List<ProductSpecItem> ParseSpecifications(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<ProductSpecItem>();

            var values = JsonSerializer.Deserialize<List<ProductSpecificationInput>>(json);
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value.Label) && !string.IsNullOrWhiteSpace(value.Value))
                .Select((value, index) => new ProductSpecItem
                {
                    Label = value.Label.Trim(),
                    Value = value.Value.Trim(),
                    SortOrder = value.SortOrder ?? index
                })
                .ToList() ?? new List<ProductSpecItem>();
        }

        private sealed class ProductSpecificationInput
        {
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public int? SortOrder { get; set; }
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.ProductRepository.FirstOrDefaultAsync(
                new ProductSpecification(id).WithIncludes(new Expression<Func<Product, object>>[] { p => p.Photos }),
                cancellationToken);
            if (product == null)
                return false;

            if (product.Photos != null)
            {
                foreach (var photo in product.Photos)
                {
                    await _imageService.DeleteImageAsync(photo.Name);
                }
            }

            await _unitOfWork.ProductRepository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductParams? productParams, CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(productParams, cancellationToken);
           
            return _mapper.Map<IReadOnlyList<ProductDto>>(products);
        }

        public Task<int> GetCountAsync(ProductParams? productParams, CancellationToken cancellationToken = default)
        {
            return _unitOfWork.ProductRepository.CountAsync(productParams, cancellationToken);
        }
    }
}
