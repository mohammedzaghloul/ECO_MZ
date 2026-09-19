using ECO.Api.Helper;
using ECO.BLL.DTO.LandingDtos;
using ECO.BLL.Services.LandingSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize(Roles = "Admin")]
    public class LandingPagesController : BaseController
    {
        private readonly ILandingService _landingService;

        public LandingPagesController(ILandingService landingService)
        {
            _landingService = landingService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var pages = await _landingService.GetAllAsync(cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<LandingPageSummaryDto>>(200, "Landing pages retrieved successfully", pages));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var page = await _landingService.GetByIdAsync(id, cancellationToken);
            if (page is null)
                return NotFound(new ResponseApi(404, "Landing page not found"));

            return Ok(new GenericResponseApi<LandingPageDto>(200, "Landing page retrieved successfully", page));
        }

        /// <summary>Public endpoint used by the storefront to render a published landing page.</summary>
        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            var page = await _landingService.GetBySlugAsync(slug, countView: true, cancellationToken);
            if (page is null || !page.IsPublished)
                return NotFound(new ResponseApi(404, "Landing page not found"));

            return Ok(new GenericResponseApi<LandingPageDto>(200, "Landing page retrieved successfully", page));
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] SaveLandingPageDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new ResponseApi(400, "Landing page data is null"));

            try
            {
                var saved = await _landingService.SaveAsync(dto, cancellationToken);
                return Ok(new GenericResponseApi<LandingPageDto>(200, "Landing page saved successfully", saved));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseApi(409, ex.Message));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var isDeleted = await _landingService.DeleteAsync(id, cancellationToken);
            if (!isDeleted)
                return NotFound(new ResponseApi(404, "Landing page not found"));

            return Ok(new ResponseApi(200, "Landing page deleted successfully"));
        }
    }
}
