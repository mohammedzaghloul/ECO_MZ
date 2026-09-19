using ECO.Api.Helper;
using ECO.BLL.DTO.CategoryDtos;
using ECO.BLL.Services.CategorySer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<CategoryDto>>(200, "Categories retrieved successfully", categories));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return NotFound(new ResponseApi(404, "Category not found"));
            }
            return Ok(new GenericResponseApi<CategoryDto>(200, "Category retrieved successfully", category));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCategoryDto category, CancellationToken cancellationToken)
        {
            if (category == null)
            {
                return BadRequest(new ResponseApi(400, "Category is null"));
            }

            var categoryDto = await _categoryService.AddAsync(category, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var isDeleted = await _categoryService.DeleteAsync(id, cancellationToken);
            if (!isDeleted)
            {
                return NotFound(new ResponseApi(404, "Category not found"));
            }
            return Ok(new ResponseApi(200, "Category deleted successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto category, CancellationToken cancellationToken)
        {
            if (category == null)
                return BadRequest(new ResponseApi(400, "Category is null"));

            if (id != category.Id)
                return BadRequest(new ResponseApi(400, "Id in route does not match Id in body"));

            var isUpdated = await _categoryService.UpdateAsync(category, cancellationToken);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, "Category not found"));

            return Ok(new ResponseApi(200, "Category updated successfully"));
        }
    }
}
