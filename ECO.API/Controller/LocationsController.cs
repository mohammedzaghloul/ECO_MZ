using ECO.Api.Helper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.Services.AdminSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ECO.Api.Controller;

public class LocationsController : BaseController
{
    private readonly ILocationCatalogService _service;
    private readonly LocationService _locations;
    public LocationsController(ILocationCatalogService service, LocationService locations)
    {
        _service = service;
        _locations = locations;
    }

    [AllowAnonymous]
    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken cancellationToken)
        => Ok(new GenericResponseApi<IReadOnlyList<LocationGovernorateDto>>(200, "Locations retrieved successfully",
            await _locations.GetAllAsync(cancellationToken)));

    [AllowAnonymous]
    [HttpGet("shipping/{cityId:int}")]
    public async Task<IActionResult> GetShipping(int cityId, CancellationToken cancellationToken)
    {
        var locations = await _locations.GetAllAsync(cancellationToken);
        var city = locations.SelectMany(x => x.Cities).FirstOrDefault(x => x.Id == cityId);
        return city is null
            ? NotFound(new ResponseApi(404, "City was not found."))
            : Ok(new GenericResponseApi<LocationCityDto>(200, "City shipping retrieved successfully", city));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("governorates")]
    public async Task<IActionResult> AddGovernorate([FromBody] SaveLocationGovernorateDto dto, CancellationToken cancellationToken)
        => Ok(new GenericResponseApi<LocationGovernorateDto>(201, "Governorate created successfully",
            await _locations.AddGovernorateAsync(dto, cancellationToken)));

    [Authorize(Roles = "Admin")]
    [HttpPut("governorates/{id:int}")]
    public async Task<IActionResult> UpdateGovernorate(int id, [FromBody] SaveLocationGovernorateDto dto, CancellationToken cancellationToken)
        => Ok(new GenericResponseApi<LocationGovernorateDto>(200, "Governorate updated successfully",
            await _locations.UpdateGovernorateAsync(id, dto, cancellationToken)));

    [Authorize(Roles = "Admin")]
    [HttpDelete("governorates/{id:int}")]
    public async Task<IActionResult> DeleteGovernorate(int id, CancellationToken cancellationToken)
    {
        await _locations.DeleteGovernorateAsync(id, cancellationToken);
        return Ok(new ResponseApi(200, "Governorate deleted successfully"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("governorates/{governorateId:int}/cities")]
    public async Task<IActionResult> AddCity(int governorateId, [FromBody] SaveLocationCityDto dto, CancellationToken cancellationToken)
        => Ok(new GenericResponseApi<LocationCityDto>(201, "City created successfully",
            await _locations.AddCityAsync(governorateId, dto, cancellationToken)));

    [Authorize(Roles = "Admin")]
    [HttpPut("cities/{id:int}")]
    public async Task<IActionResult> UpdateCity(int id, [FromBody] SaveLocationCityDto dto, CancellationToken cancellationToken)
        => Ok(new GenericResponseApi<LocationCityDto>(200, "City updated successfully",
            await _locations.UpdateCityAsync(id, dto, cancellationToken)));

    [Authorize(Roles = "Admin")]
    [HttpDelete("cities/{id:int}")]
    public async Task<IActionResult> DeleteCity(int id, CancellationToken cancellationToken)
    {
        await _locations.DeleteCityAsync(id, cancellationToken);
        return Ok(new ResponseApi(200, "City deleted successfully"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{key}")]
    public async Task<IActionResult> Save(string key, [FromBody] SaveLocationCatalogDto dto, CancellationToken cancellationToken)
    {
        dto.Key = key;
        try
        {
            return Ok(new GenericResponseApi<LocationCatalogDto>(200, "Locations saved successfully",
                await _service.SaveAsync(dto, cancellationToken)));
        }
        catch (JsonException ex)
        {
            return BadRequest(new ResponseApi(400, $"Invalid JSON: {ex.Message}"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseApi(400, ex.Message));
        }
    }
}
