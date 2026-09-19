using ECO.DAL.Sharing;
using ECO.DAL.Entities.Product;
using System.Linq.Expressions;
using ECO.BLL.DTO.ProductDtos;

namespace ECO.BLL.Services.ProductServices
{
    public interface IProductService
    {
        Task<ProductDto> AddAsync(AddProductDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductParams? productParams, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductDto>> GetAllAsync(Expression<Func<Product, object>>[] includes, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetByIdAsync(int id, Expression<Func<Product, object>>[] includes, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateProductDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<int> GetCountAsync(ProductParams? productParams, CancellationToken cancellationToken = default);
    }
}
