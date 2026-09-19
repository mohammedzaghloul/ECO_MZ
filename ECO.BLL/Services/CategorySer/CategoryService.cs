using AutoMapper;
using ECO.BLL.DTO.CategoryDtos;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
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

        public async Task<CategoryDto> AddAsync(AddCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var category = _mapper.Map<Category>(dto);
            await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _unitOfWork.Repository<Category>()
                .ListAsync(new CategorySpecification(), cancellationToken);
            return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(Expression<Func<Category, object>>[] includes, CancellationToken cancellationToken = default)
        {
            var specification = new CategorySpecification().WithIncludes(includes);
            var categories = await _unitOfWork.Repository<Category>().ListAsync(specification, cancellationToken);
            return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, cancellationToken);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, Expression<Func<Category, object>>[] includes, CancellationToken cancellationToken = default)
        {
            var specification = new CategorySpecification(id).WithIncludes(includes);
            var category = await _unitOfWork.Repository<Category>().FirstOrDefaultAsync(specification, cancellationToken);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task<bool> UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id, cancellationToken);
            if (category is null)
                return false;

            _mapper.Map(dto, category);
            await _unitOfWork.Repository<Category>().UpdateAsync(category, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, cancellationToken);
            if (category is null)
                return false;

            await _unitOfWork.Repository<Category>().DeleteAsync(id, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
    }
}
