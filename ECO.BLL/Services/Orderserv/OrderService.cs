
using AutoMapper;
using ECO.BLL.DTO.Order;
using ECO.BLL.DTO.OrderDtos;
using ECO.BLL.Services.Basket;
using ECO.BLL.Constants;
using ECO.DAL.Entities;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Entities.Product;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using RedisException = StackExchange.Redis.RedisException;
using ECO.BLL.Services.Payment;
using ECO.BLL.Services.Notifications;
using ECO.BLL.Services.Email;

namespace ECO.BLL.Services.Orderserv
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBasketService _basketService;
        private readonly IMapper mapper;
        private readonly ILogger<OrderService> logger;
        private readonly IPaymentService paymentService;
        private readonly INotificationService notificationService;
        private readonly BasketSettings settings;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ECO.DAL.Data.AppDbContext _db;

        public OrderService(
            IUnitOfWork unitOfWork,
            IBasketService basketService,
            IMapper mapper,
            ILogger<OrderService> logger,
            IOptions<BasketSettings> options,IPaymentService paymentService,
            INotificationService notificationService,
            IEmailService emailService,
            IConfiguration configuration,
            ECO.DAL.Data.AppDbContext db)
        {
            this.unitOfWork = unitOfWork;
            _basketService = basketService;
            this.mapper = mapper;
            this.logger = logger;
            this.paymentService = paymentService;
            this.notificationService = notificationService;
            _emailService = emailService;
            _configuration = configuration;
            settings = options.Value;
            _db = db;
        }
        public async Task<OrderDto> CreateAsync( OrderDto orderDto, string buyerEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            if (string.IsNullOrWhiteSpace(buyerEmail))
                throw new InvalidOperationException("Authenticated user email is required");

            if (orderDto.ShippingAddressDto == null)
                throw new ArgumentException("Shipping address is required", nameof(orderDto));

            if (orderDto.GovernorateId <= 0 || orderDto.CityId <= 0)
                throw new ArgumentException("A valid governorate and city are required", nameof(orderDto));

            if (string.IsNullOrWhiteSpace(orderDto.BasketId))
                throw new ArgumentException("Basket id is required", nameof(orderDto));

            var existingOrder = await unitOfWork.Repository<Order>()
                .FirstOrDefaultAsync(
                    new OrderSpecification(buyerEmail, orderDto.BasketId),
                    cancellationToken);
            if (existingOrder != null)
                throw new InvalidOperationException("An order has already been created for this basket");

            // Get Basket
            var basket = await _basketService.GetBasketAsync(orderDto.BasketId, cancellationToken);

            if (basket == null || !basket.BasketItems.Any())
                throw new InvalidOperationException($"Basket '{orderDto.BasketId}' is empty");

            // Stripe (default) | InstaPay | VodafoneCash | COD
            var paymentMethod = string.IsNullOrWhiteSpace(orderDto.PaymentMethod)
                ? (string.IsNullOrWhiteSpace(basket.PaymentIntentId) ? "COD" : "Stripe")
                : orderDto.PaymentMethod.Trim();
            var isStripe = paymentMethod.Equals("Stripe", StringComparison.OrdinalIgnoreCase);

            // Get Delivery Method
            var deliveryMethod = await unitOfWork
                .Repository<DeliveryMethod>()
                .GetByIdAsync(orderDto.DeliveryMethodId, cancellationToken);

            if (deliveryMethod == null)
                throw new InvalidOperationException(
                    $"Delivery method '{orderDto.DeliveryMethodId}' was not found");

            var city = await unitOfWork.Repository<LocationCity>()
                .GetByIdAsync(orderDto.CityId, cancellationToken);
            var governorate = await unitOfWork.Repository<LocationGovernorate>()
                .GetByIdAsync(orderDto.GovernorateId, cancellationToken);
            if (governorate == null || !governorate.IsActive
                || city == null || !city.IsActive || city.GovernorateId != orderDto.GovernorateId)
                throw new InvalidOperationException("The selected city does not belong to the selected governorate");
            if (!city.ShippingAvailable)
                throw new InvalidOperationException("Shipping is unavailable for the selected city");
            if (city.ShippingPrice < 0 || city.DeliveryDays <= 0)
                throw new InvalidOperationException("Shipping configuration is invalid for the selected city");

            cancellationToken.ThrowIfCancellationRequested();

            var productIds = basket.BasketItems.Select(item => item.Id).Distinct().ToList();
            var products = await unitOfWork.ProductRepository.GetByIdsAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(product => product.Id);
            var orderItems = new List<OrderItem>();
            var missingProductIds = new List<int>();

            foreach (var item in basket.BasketItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.Quantity <= 0 || item.Quantity > settings.MaxQuantityPerItem)
                    throw new ArgumentException(
                        OrderErrorMessages.InvalidQuantity(1, settings.MaxQuantityPerItem));

                if (!productsById.TryGetValue(item.Id, out var product))
                {
                    missingProductIds.Add(item.Id);
                    continue;
                }

                orderItems.Add(new OrderItem(
                    product.Id,
                    product.NewPrice,
                    item.Quantity,
                    product.Name,
                    item.Image));
            }

            if (!orderItems.Any())
                throw new InvalidOperationException("No valid products found in basket");

            if (missingProductIds.Any())
                throw new InvalidOperationException(
                    OrderErrorMessages.ProductsUnavailableFor(missingProductIds));

            // Calculate SubTotal
            var subTotal = orderItems.Sum(x => x.Price * x.Quantity);

            // Apply coupon from the basket (re-validated server-side).
            var discountAmount = 0m;
            if (!string.IsNullOrWhiteSpace(basket.CouponCode))
            {
                var coupon = await unitOfWork.Repository<Discount>().FirstOrDefaultAsync(
                    new DiscountSpecification(basket.CouponCode), cancellationToken);

                var couponValid = coupon is { IsActive: true }
                    && (coupon.ExpiryDate == null || coupon.ExpiryDate.Value > DateTime.UtcNow)
                    && (coupon.MaxUses == null || coupon.UsedCount < coupon.MaxUses.Value);

                if (couponValid && coupon is not null)
                {
                    discountAmount = coupon.IsPercentage
                        ? Math.Round(subTotal * coupon.Value / 100m, 2)
                        : Math.Min(coupon.Value, subTotal);
                    coupon.UsedCount++;
                    await unitOfWork.Repository<Discount>().UpdateAsync(coupon, cancellationToken);
                }
            }

            // Map Shipping Address
            var shippingAddress = mapper.Map<ShippingAddress>(orderDto.ShippingAddressDto);
            if (string.IsNullOrWhiteSpace(shippingAddress.Country))
            {
                shippingAddress.Country = "Egypt";
            }

            if (isStripe && !string.IsNullOrWhiteSpace(basket.PaymentIntentId))
            {
                var checkExOrder = await unitOfWork.Repository<Order>().FirstOrDefaultAsync(OrderSpecification.ByPaymentIntentId(basket.PaymentIntentId));
                if (checkExOrder != null)
                {
                    await unitOfWork.Repository<Order>().DeleteAsync(checkExOrder.Id);
                    await paymentService.CreateOrUpadetPaymentAsync(basket.Id, deliveryMethod.Id, cancellationToken);
                }
            }

            var paymentIntentId = isStripe
                ? (!string.IsNullOrWhiteSpace(basket.PaymentIntentId) ? basket.PaymentIntentId : paymentMethod)
                : paymentMethod;

            // Create Order — non-Stripe methods carry the method name in place of a payment intent.
            var order = new Order(
                buyerEmail,
                orderDto.BasketId,
                subTotal,
                shippingAddress,
                deliveryMethod,
                orderItems,
                paymentIntentId
            );
            order.ShippingPrice = city.ShippingPrice;

            if (isStripe && !string.IsNullOrWhiteSpace(basket.PaymentIntentId))
            {
                // The client only calls CreateOrder after Stripe confirmed the payment,
                // so the order is born paid rather than pending.
                order.Status = Status.PaymentRecevied;
            }
            else
            {
                // COD / InstaPay / VodafoneCash stay Pending until money or goods change hands.
                order.Status = Status.Pending;
            }
            order.Discount = discountAmount;
            order.PaymentMethod = paymentMethod;

            var transaction = settings.EnableStockValidation
                ? await unitOfWork.BeginTransactionAsync(cancellationToken)
                : null;
            try
            {
                if (settings.EnableStockValidation)
                {
                    var insufficientStockProductIds = new List<int>();
                    foreach (var item in basket.BasketItems)
                    {
                        if (!productsById[item.Id].TrackStock)
                            continue;

                        var decremented = await unitOfWork.ProductRepository
                            .TryDecrementStockAsync(item.Id, item.Quantity, cancellationToken);
                        if (!decremented)
                            insufficientStockProductIds.Add(item.Id);
                    }

                    if (insufficientStockProductIds.Count > 0)
                    {
                        throw new InvalidOperationException(
                            OrderErrorMessages.InsufficientStockFor(insufficientStockProductIds));
                    }
                }

                await unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);
                await unitOfWork.CompleteAsync(cancellationToken);
                if (transaction != null)
                    await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                if (transaction != null)
                    await transaction.RollbackAsync(CancellationToken.None);
                throw new InvalidOperationException(
                    "An order has already been created for this basket", ex);
            }
            catch
            {
                if (transaction != null)
                    await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }

            bool basketDeleted;
            try
            {
                basketDeleted = await _basketService.DeleteAsync(orderDto.BasketId, cancellationToken);
            }
            catch (RedisException ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to delete basket {BasketId} after order {OrderId} was created",
                    orderDto.BasketId,
                    order.Id);
                basketDeleted = false;
            }

            if (!basketDeleted)
                logger.LogWarning(
                    "Basket {BasketId} was not deleted after order {OrderId} was created",
                    orderDto.BasketId,
                    order.Id);

            try
            {
                await notificationService.CreateAsync(
                    buyerEmail,
                    "Order created",
                    $"Your order #{order.Id} has been created successfully.",
                    "Order",
                    order.Id,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create notification for order {OrderId}", order.Id);
            }

            try
            {
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                var storeSettings = await _db.StoreSettings.AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);
                var lang = storeSettings?.EmailDefaultLanguage == "en" ? "en" : "ar";
                var appearance = EmailAppearanceOptions.From(storeSettings);
                var copy = EmailTemplateCopy.Resolve(EmailTemplateCopy.Order, lang,
                    await _db.EmailTemplateSettings.AsNoTracking().FirstOrDefaultAsync(
                        x => x.TemplateType == EmailTemplateCopy.Order && x.Language == lang, cancellationToken));
                var subject = EmailTemplateCopy.FillSubject(copy.Subject, appearance.BrandName,
                    order.ShippingAddress?.FirstName, order.Id);
                await _emailService.SendEmail(new ECO.BLL.DTO.Auth.EmailDto(
                    buyerEmail,
                    buyerEmail,
                    subject,
                    OrderEmailTemplate.Build(order, frontendUrl, appearance, copy)));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send order confirmation email for order {OrderId}", order.Id);
            }

            return mapper.Map<OrderDto>(order);
        }
        public async Task<IReadOnlyList<OrderToReturnDto>> GetAllAsync( CancellationToken cancellationToken = default)
        {
            var orders = await unitOfWork.Repository<Order>().ListAsync(
                new OrderSpecification(),
                cancellationToken);
            return mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders);
        }

        public async Task<IReadOnlyList<OrderToReturnDto>> GetAllOrderForUserAsync(  string BuyerEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(BuyerEmail))
                throw new ArgumentException("Buyer email is required", nameof(BuyerEmail));

            var orders = await unitOfWork.Repository<Order>().ListAsync(
                new OrderSpecification(BuyerEmail),
                cancellationToken);
            return mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders);
        }

        public async Task<IReadOnlyList<DeliveryMethodDto>> GetDeliveryMethodAsync(CancellationToken cancellationToken = default)
        {
            var methods = await unitOfWork.Repository<DeliveryMethod>()
                .ListAsync(cancellationToken: cancellationToken);
            return mapper.Map<IReadOnlyList<DeliveryMethodDto>>(methods);
        }

        public async Task<OrderToReturnDto?> GetOrderById( int Id,string Email,  CancellationToken cancellationToken = default)
        {
            if (Id <= 0)
                throw new ArgumentException("Order id must be greater than zero", nameof(Id));
            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Buyer email is required", nameof(Email));

            var order = await unitOfWork.Repository<Order>().FirstOrDefaultAsync(
                new OrderSpecification(Id, Email),
                cancellationToken);
            return mapper.Map<OrderToReturnDto?>(order);
        }
    }
}
