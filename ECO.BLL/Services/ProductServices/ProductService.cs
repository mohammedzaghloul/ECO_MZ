using AutoMapper;
using ECO.BLL.DTO;
using ECO.DAL.Sharing;
using ECO.DAL.Entites.Product;
using ECO.DAL.Interfaces;
using System.Linq.Expressions;
using ECO.BLL.Services.Upload;

namespace ECO.BLL.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageManagementService _imageService;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IImageManagementService imageService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _mapper = mapper;
        }

        public async Task<ProductDto> AddAsync(AddProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.CompleteAsync();

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
                    await _unitOfWork.Repository<Photo>().AddAsync(photo);
                }

                await _unitOfWork.CompleteAsync();
            }

            var savedProduct = await _unitOfWork.ProductRepository  
                .GetByIdAsync(product.Id, p => p.Category, p => p.Photos);

            return _mapper.Map<ProductDto>(savedProduct);
        }
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(params Expression<Func<Product, object>>[] includes)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(includes);
            return _mapper.Map<IReadOnlyList<ProductDto>>(products);
        }
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id, p => p.Category, p => p.Photos);
            return _mapper.Map<ProductDto?>(product);
        }
        public async Task<ProductDto?> GetByIdAsync(int id, params Expression<Func<Product, object>>[] includes)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id, includes);
            return _mapper.Map<ProductDto?>(product);
        }
        public async Task<bool> UpdateAsync(UpdateProductDto dto)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(dto.Id, p => p.Photos);
            if (product == null)
                return false;

            _mapper.Map(dto, product);

            if (dto.Photos != null && dto.Photos.Count > 0)
            {
               
                    foreach (var oldPhoto in product.Photos)
                    {
                        await _imageService.DeleteImageAsync(oldPhoto.Name);
                        await _unitOfWork.Repository<Photo>().DeleteAsync(oldPhoto.Id);
                    }
                

                var newImageUrls = await _imageService.AddImageAsync(dto.Photos, "Products");
                foreach (var imageUrl in newImageUrls)
                {
                    var photo = new Photo
                    {
                        Name = imageUrl,
                        ProductId = product.Id
                    };
                    await _unitOfWork.Repository<Photo>().AddAsync(photo);
                }
            }

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id, p => p.Photos);
            if (product == null)
                return false;

            if (product.Photos != null)
            {
                foreach (var photo in product.Photos)
                {
                    await _imageService.DeleteImageAsync(photo.Name);
                }
            }

            await _unitOfWork.ProductRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return true;
        }
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductParams? productParams)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(productParams);
           
            return _mapper.Map<IReadOnlyList<ProductDto>>(products);
        }

        public Task<int> GetCountAsync(ProductParams? productParams)
        {
            return _unitOfWork.ProductRepository.CountAsync(productParams);
        }
    }
}
