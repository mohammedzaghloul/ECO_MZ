using AutoMapper;
using ECO.BLL.DTO;
using ECO.BLL.DTO.CategoryDtos;
using ECO.DAL.Entites.Product;
using ECO.DAL.Interfaces;
using System.Linq.Expressions;

namespace ECO.BLL.Services.CategorySer
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CategoryDto> AddAsync(AddCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(params Expression<Func<Category, object>>[] includes)
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync(includes);
            return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, params Expression<Func<Category, object>>[] includes)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, includes);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task<bool> UpdateAsync(UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id);
            if (category is null)
                return false;

            _mapper.Map(dto, category);
            await _unitOfWork.Repository<Category>().UpdateAsync(category);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category is null)
                return false;

            await _unitOfWork.Repository<Category>().DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
