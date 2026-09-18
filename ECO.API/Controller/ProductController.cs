using ECO.Api.Helper;
using ECO.BLL.DTO;
using ECO.BLL.Services.ProductServices;
using ECO.DAL.Sharing;
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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] ProductParams? productParams)
        {
            var products = await _productService.GetAllAsync(productParams);

            var totalCount = await _productService.GetCountAsync(productParams);

            var pageNumber = productParams?.PageNumber ?? 1;
            var pageSize = productParams?.PageSize ?? 10;

            return Ok(new Pagination<ProductDto>(
                pageNumber,
                totalCount,
                pageSize,
                products
            ));
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
