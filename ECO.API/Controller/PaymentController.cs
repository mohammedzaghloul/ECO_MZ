using ECO.BLL.Services.Payment;
using ECO.DAL.Entities.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECO.Api.Controller
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }
        [HttpPost("Create")]
        public async Task<ActionResult<CustomerBasket>> CreatePayment(string basketId,int? deliverymethodid,CancellationToken cancellationToken)
        {
            return await paymentService.CreateOrUpadetPaymentAsync(basketId, deliverymethodid,cancellationToken);
        }
    }
}
