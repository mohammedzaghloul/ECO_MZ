using ECO.BLL.DTO.AdminDtos;
using ECO.DAL.Data;
using ECO.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECO.BLL.Services.AdminSer;

public sealed class LocationService
{
    private readonly AppDbContext _db;

    public LocationService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<LocationGovernorateDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.LocationGovernorates.AsNoTracking()
            .Include(x => x.Cities)
            .OrderBy(x => x.En)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

    public async Task<LocationGovernorateDto> AddGovernorateAsync(SaveLocationGovernorateDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.Value, dto.En, dto.Ar);
        var governorate = new LocationGovernorate { Value = dto.Value.Trim(), En = dto.En.Trim(), Ar = dto.Ar.Trim() };
        _db.LocationGovernorates.Add(governorate);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(governorate);
    }

    public async Task<LocationGovernorateDto> UpdateGovernorateAsync(int id, SaveLocationGovernorateDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.Value, dto.En, dto.Ar);
        var governorate = await _db.LocationGovernorates.Include(x => x.Cities).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Governorate was not found.");
        governorate.Value = dto.Value.Trim();
        governorate.En = dto.En.Trim();
        governorate.Ar = dto.Ar.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(governorate);
    }

    public async Task DeleteGovernorateAsync(int id, CancellationToken cancellationToken = default)
    {
        var governorate = await _db.LocationGovernorates.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException("Governorate was not found.");
        _db.LocationGovernorates.Remove(governorate);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LocationCityDto> AddCityAsync(int governorateId, SaveLocationCityDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.Value, dto.En, dto.Ar);
        if (!await _db.LocationGovernorates.AnyAsync(x => x.Id == governorateId, cancellationToken))
            throw new KeyNotFoundException("Governorate was not found.");
        ValidateShipping(dto);
        var city = new LocationCity
        {
            GovernorateId = governorateId,
            Value = dto.Value.Trim(),
            En = dto.En.Trim(),
            Ar = dto.Ar.Trim(),
            ShippingPrice = dto.ShippingPrice,
            DeliveryDays = dto.DeliveryDays,
            ShippingAvailable = dto.ShippingAvailable
        };
        _db.LocationCities.Add(city);
        await _db.SaveChangesAsync(cancellationToken);
        return new LocationCityDto { Id = city.Id, Value = city.Value, En = city.En, Ar = city.Ar, ShippingPrice = city.ShippingPrice, DeliveryDays = city.DeliveryDays, ShippingAvailable = city.ShippingAvailable };
    }

    public async Task DeleteCityAsync(int id, CancellationToken cancellationToken = default)
    {
        var city = await _db.LocationCities.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException("City was not found.");
        _db.LocationCities.Remove(city);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LocationCityDto> UpdateCityAsync(int id, SaveLocationCityDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.Value, dto.En, dto.Ar);
        ValidateShipping(dto);
        var city = await _db.LocationCities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("City was not found.");
        city.Value = dto.Value.Trim();
        city.En = dto.En.Trim();
        city.Ar = dto.Ar.Trim();
        city.ShippingPrice = dto.ShippingPrice;
        city.DeliveryDays = dto.DeliveryDays;
        city.ShippingAvailable = dto.ShippingAvailable;
        await _db.SaveChangesAsync(cancellationToken);
        return new LocationCityDto { Id = city.Id, Value = city.Value, En = city.En, Ar = city.Ar, ShippingPrice = city.ShippingPrice, DeliveryDays = city.DeliveryDays, ShippingAvailable = city.ShippingAvailable };
    }

    private static void Validate(string value, string en, string ar)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(en) || string.IsNullOrWhiteSpace(ar))
            throw new ArgumentException("Value, English name, and Arabic name are required.");
    }

    private static void ValidateShipping(SaveLocationCityDto dto)
    {
        if (dto.ShippingPrice < 0)
            throw new ArgumentException("Shipping price cannot be negative.");
        if (dto.DeliveryDays < 0)
            throw new ArgumentException("Delivery days cannot be negative.");
    }

    private static LocationGovernorateDto ToDto(LocationGovernorate x) => new()
    {
        Id = x.Id, Value = x.Value, En = x.En, Ar = x.Ar,
        Cities = x.Cities.OrderBy(c => c.En).Select(c => new LocationCityDto { Id = c.Id, Value = c.Value, En = c.En, Ar = c.Ar, ShippingPrice = c.ShippingPrice, DeliveryDays = c.DeliveryDays, ShippingAvailable = c.ShippingAvailable }).ToList()
    };
}
