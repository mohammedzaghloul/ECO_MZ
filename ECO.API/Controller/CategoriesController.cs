using ECO.Api.Helper;
using ECO.BLL.DTO;
using ECO.BLL.DTO.CategoryDtos;
using ECO.BLL.Services.CategorySer;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(new GenericResponseApi<IReadOnlyList<CategoryDto>>(200, "Categories retrieved successfully", categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new ResponseApi(404, "Category not found"));
            }
            return Ok(new GenericResponseApi<CategoryDto>(200, "Category retrieved successfully", category));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCategoryDto category)
        {
            if (category == null)
            {
                return BadRequest(new ResponseApi(400, "Category is null"));
            }

            var categoryDto = await _categoryService.AddAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _categoryService.DeleteAsync(id);
            if (!isDeleted)
            {
                return NotFound(new ResponseApi(404, "Category not found"));
            }
            return Ok(new ResponseApi(200, "Category deleted successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto category)
        {
            if (category == null)
                return BadRequest(new ResponseApi(400, "Category is null"));

            if (id != category.Id)
                return BadRequest(new ResponseApi(400, "Id in route does not match Id in body"));

            var isUpdated = await _categoryService.UpdateAsync(category);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, "Category not found"));

            return Ok(new ResponseApi(200, "Category updated successfully"));
        }
    }
}
