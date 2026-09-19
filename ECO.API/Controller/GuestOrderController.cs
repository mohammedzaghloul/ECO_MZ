using ECO.Api.Helper;
using ECO.BLL.DTO.OrderDtos;
using ECO.BLL.Services.Orderserv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECO.Api.Controller;

[AllowAnonymous]
public class GuestOrderController : BaseController
{
    private readonly IGuestOrderService _guestOrderService;

    public GuestOrderController(IGuestOrderService guestOrderService)
    {
        _guestOrderService = guestOrderService;
    }

    [HttpPost("Create")]
    [EnableRateLimiting("guest-orders")]
    public async Task<IActionResult> Create( GuestOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _guestOrderService.CreateAsync(request, cancellationToken);
            return Ok(new GenericResponseApi<GuestOrderResponseDto>(
                201, "Order created successfully", result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseApi(400, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ResponseApi(409, ex.Message));
        }
    }
}
