using ECO.BLL.DTO;
using ECO.DAL.Sharing;
using ECO.DAL.Entites.Product;
using System.Linq.Expressions;

namespace ECO.BLL.Services.ProductServices
{
    public interface IProductService
    {
        Task<ProductDto> AddAsync(AddProductDto dto);
        Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductParams? productParams);
        Task<IReadOnlyList<ProductDto>> GetAllAsync(params Expression<Func<Product, object>>[] includes);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto?> GetByIdAsync(int id, params Expression<Func<Product, object>>[] includes);
        Task<bool> UpdateAsync(UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountAsync(ProductParams? productParams);
    }
}
