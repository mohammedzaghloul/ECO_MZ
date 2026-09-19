using ECO.BLL.DTO.CategoryDtos;
using ECO.DAL.Entities.Product;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.BLL.Services.CategorySer
{
    public interface ICategoryService
    {
        Task<CategoryDto> AddAsync(AddCategoryDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CategoryDto>> GetAllAsync(Expression<Func<Category, object>>[] includes, CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetByIdAsync(int id, Expression<Func<Category, object>>[] includes, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
