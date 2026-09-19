using ECO.Api.Helper;
using ECO.BLL.DTO.Order;
using ECO.BLL.DTO.OrderDtos;
using ECO.BLL.Services.Orderserv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECO.Api.Controller
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        private string GetBuyerEmail()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("A valid authenticated user is required.");
            return email;
        }
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> Create( OrderDto orderDto, CancellationToken cancellationToken) 
        {
            var Email = GetBuyerEmail();

            try
            {
                var order = await _orderService.CreateAsync(orderDto, Email, cancellationToken);
                return Ok(new GenericResponseApi<OrderDto>(201, "Create Order", order));
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ResponseApi(409, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseApi(500, ex.Message));
            }
        }
        [HttpGet("GetAllOrderForUser")]
        public async Task<IActionResult> GetOrderForUser(CancellationToken cancellationToken)
        {
            var Email = GetBuyerEmail();
            var  allOrders = await _orderService.GetAllOrderForUserAsync(Email, cancellationToken);
            if(!allOrders.Any()) 
                return NotFound(new ResponseApi(404,"Not Found Orders"));
            return Ok(new GenericResponseApi<IReadOnlyList<OrderToReturnDto>>(200, "All Orders", allOrders));
        }
        [HttpGet("GetOrderByIdForUser/{id}")]
        public async Task<IActionResult> GetOrderById(int id, CancellationToken cancellationToken)
        {
            var email = GetBuyerEmail();
            var order =await _orderService.GetOrderById(id, email, cancellationToken);
           if (order == null)
                    return NotFound(new ResponseApi(404, $"Order with id {id} was not found"));
            return Ok(new GenericResponseApi<OrderToReturnDto> (200,"",order));
        }
        [AllowAnonymous]
        [HttpGet("GetDeliveryMethod")]
        public async Task<IActionResult> GetDelivery(CancellationToken cancellationToken)
        {
            try
            {
                var methods = await _orderService.GetDeliveryMethodAsync(cancellationToken);
                return Ok(new GenericResponseApi<IReadOnlyList<DeliveryMethodDto>>(
                    200,
                    "Delivery methods",
                    methods));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
        }
    }
}
