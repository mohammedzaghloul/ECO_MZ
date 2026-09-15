using ECO.BLL.DTO;
using ECO.BLL.DTO.CategoryDtos;
using ECO.DAL.Entites.Product;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ECO.BLL.Services.CategorySer
{
    public interface ICategoryService
    {
        Task<CategoryDto> AddAsync(AddCategoryDto dto);
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<IReadOnlyList<CategoryDto>> GetAllAsync(params Expression<Func<Category, object>>[] includes);
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto?> GetByIdAsync(int id, params Expression<Func<Category, object>>[] includes);
        Task<bool> UpdateAsync(UpdateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
