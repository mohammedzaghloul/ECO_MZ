using ECO.BLL.DTO.LandingDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.LandingSer
{
    public interface ILandingService
    {
        Task<IReadOnlyList<LandingPageSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<LandingPageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<LandingPageDto?> GetBySlugAsync(string slug, bool countView = false, CancellationToken cancellationToken = default);
        Task<LandingPageDto> SaveAsync(SaveLandingPageDto dto, CancellationToken cancellationToken = default);
        Task<LandingPageDto> GenerateDefaultForProductAsync(GenerateDefaultForProductDto generateDefaultForProduct, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
