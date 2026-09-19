using System.Text.Json;
using ECO.BLL.DTO.AdminDtos;
using ECO.DAL.Entities;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;

namespace ECO.BLL.Services.AdminSer;

public sealed class LocationCatalogService : ILocationCatalogService
{
    private readonly IUnitOfWork _unitOfWork;

    public LocationCatalogService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<LocationCatalogDto> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var catalog = await _unitOfWork.Repository<LocationCatalog>()
            .FirstOrDefaultAsync(new LocationCatalogSpecification(key), cancellationToken);
        if (catalog is null)
            throw new KeyNotFoundException($"Location catalog '{key}' was not found.");
        return ToDto(catalog);
    }

    public async Task<LocationCatalogDto> SaveAsync(SaveLocationCatalogDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Key))
            throw new ArgumentException("Catalog key is required.", nameof(dto.Key));

        using var document = JsonDocument.Parse(dto.JsonContent);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Location JSON must be an array of governorates.", nameof(dto.JsonContent));

        var catalog = await _unitOfWork.Repository<LocationCatalog>()
            .FirstOrDefaultAsync(new LocationCatalogSpecification(dto.Key), cancellationToken);
        if (catalog is null)
        {
            catalog = new LocationCatalog { Key = dto.Key.Trim().ToLowerInvariant() };
            await _unitOfWork.Repository<LocationCatalog>().AddAsync(catalog, cancellationToken);
        }

        catalog.JsonContent = JsonSerializer.Serialize(
            document.RootElement,
            new JsonSerializerOptions { WriteIndented = true });
        catalog.UpdatedAtUtc = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync(cancellationToken);
        return ToDto(catalog);
    }

    private static LocationCatalogDto ToDto(LocationCatalog catalog) => new()
    {
        Id = catalog.Id,
        Key = catalog.Key,
        JsonContent = catalog.JsonContent,
        UpdatedAtUtc = catalog.UpdatedAtUtc
    };
}

internal sealed class LocationCatalogSpecification : BaseSpecification<LocationCatalog>
{
    public LocationCatalogSpecification(string key)
        : base(catalog => catalog.Key == key.Trim().ToLower()) { }
}
