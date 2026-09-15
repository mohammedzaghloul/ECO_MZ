using ECO.Api.Helper;
using ECO.BLL.DTO;
using ECO.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(new GenericResponseApi<IReadOnlyList<ProductDto>>(200, "Products retrieved successfully", products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var productDto = await _productService.GetByIdAsync(id);
            if (productDto == null)
            {
                return NotFound(new ResponseApi(404, "Product not found"));
            }
            return Ok(new GenericResponseApi<ProductDto>(200, "Product retrieved successfully", productDto));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddProductDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ResponseApi(400, "Product data is null"));
            }

            var productDto = await _productService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDto dto)
        {
            if (dto == null)
                return BadRequest(new ResponseApi(400, "Product data is null"));

            dto.Id = id;

            var isUpdated = await _productService.UpdateAsync(dto);
            if (!isUpdated)
                return NotFound(new ResponseApi(404, "Product not found"));

            return Ok(new ResponseApi(200, "Product updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _productService.DeleteAsync(id);
            if (!isDeleted)
                return NotFound(new ResponseApi(404, "Product not found"));

            return Ok(new ResponseApi(200, "Product deleted successfully"));
        }
    }
}
