using ECO.BLL.DTO;
using ECO.DAL.Entites.Product;
using System.Linq.Expressions;

namespace ECO.BLL.Services
{
    public interface IProductService
    {
        Task<ProductDto> AddAsync(AddProductDto dto);
        Task<IReadOnlyList<ProductDto>> GetAllAsync();
        Task<IReadOnlyList<ProductDto>> GetAllAsync(params Expression<Func<Product, object>>[] includes);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto?> GetByIdAsync(int id, params Expression<Func<Product, object>>[] includes);
        Task<bool> UpdateAsync(UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
