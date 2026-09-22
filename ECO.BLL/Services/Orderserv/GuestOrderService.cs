using ECO.BLL.DTO.OrderDtos;
using ECO.DAL.Entities;
using ECO.DAL.Entities.Landing;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ECO.BLL.Services.Orderserv;

public class GuestOrderService : IGuestOrderService
{
    private const string GuestEmail = "guest@landing.local";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public GuestOrderService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<GuestOrderResponseDto> CreateAsync(
        GuestOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var duplicateKey = $"guest-order:{request.LandingPageId}:{request.ProductId}:{request.Phone}";
        if (_cache.TryGetValue(duplicateKey, out _))
            throw new InvalidOperationException("A similar order was submitted recently.");

        var landingPage = await _unitOfWork.Repository<LandingPage>()
            .FirstOrDefaultAsync(new LandingPageSpecification(request.LandingPageId), cancellationToken);
        if (landingPage is null || !landingPage.IsPublished || landingPage.ProductId != request.ProductId)
            throw new InvalidOperationException("The landing page or product is not available.");

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            throw new InvalidOperationException("The product is not available.");
        if (product.TrackStock && (!product.StockQuantity.HasValue || product.StockQuantity.Value < request.Quantity))
            throw new InvalidOperationException("The requested quantity is not available.");
        if (!string.IsNullOrWhiteSpace(request.SelectedSize))
        {
            var sizeStockSpec = product.Specifications.FirstOrDefault(item =>
                item.Label.Contains("مخزون", StringComparison.OrdinalIgnoreCase) &&
                item.Label.Contains("مقاس", StringComparison.OrdinalIgnoreCase));
            if (sizeStockSpec is not null &&
                TryGetSizeStock(sizeStockSpec.Value, request.SelectedSize.Trim(), out var sizeStock) &&
                sizeStock < request.Quantity)
                throw new InvalidOperationException("The selected size is not available in the requested quantity.");
        }

        var city = await _unitOfWork.Repository<LocationCity>()
            .GetByIdAsync(request.CityId, cancellationToken);
        var governorate = await _unitOfWork.Repository<LocationGovernorate>()
            .GetByIdAsync(request.GovernorateId, cancellationToken);
        if (governorate is null || !governorate.IsActive ||
            city is null || !city.IsActive || city.GovernorateId != request.GovernorateId ||
            !city.ShippingAvailable)
            throw new InvalidOperationException("The selected delivery location is not available.");

        var deliveryMethod = (await _unitOfWork.Repository<DeliveryMethod>()
            .ListAsync(null, cancellationToken)).FirstOrDefault();
        if (deliveryMethod is null)
            throw new InvalidOperationException("No delivery method is configured.");

        // Attach delivery method to prevent EF from trying to insert it
        _unitOfWork.Repository<DeliveryMethod>().Attach(deliveryMethod);

        var orderItem = new OrderItem(
            product.Id,
            product.NewPrice,
            request.Quantity,
            product.Name,
            product.Photos.FirstOrDefault()?.Name ?? string.Empty)
        {
            MainImage = product.Photos.FirstOrDefault()?.Name ?? string.Empty
        };

        var order = new Order(
            GuestEmail,
            $"landing-{Guid.NewGuid():N}",
            product.NewPrice * request.Quantity,
            new ShippingAddress(
                request.CustomerName.Trim(),
                string.Empty,
                city.Value,
                string.Empty,
                request.Address.Trim(),
                governorate.Value,
                "Egypt"),
            deliveryMethod,
            new[] { orderItem },
            "COD")
        {
            BuyerPhone = request.Phone,
            CustomerName = request.CustomerName.Trim(),
            LandingPageId = request.LandingPageId,
            PaymentMethod = "COD",
            ShippingPrice = city.ShippingPrice,
            Status = Status.Pending
        };

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        if (product.TrackStock)
        {
            var stockUpdated = await _unitOfWork.ProductRepository
                .TryDecrementStockAsync(product.Id, request.Quantity, cancellationToken);
            if (!stockUpdated)
                throw new InvalidOperationException("The requested quantity is no longer available.");
        }
        if (!string.IsNullOrWhiteSpace(request.SelectedSize))
        {
            var sizeStockSpec = product.Specifications.FirstOrDefault(item =>
                item.Label.Contains("مخزون", StringComparison.OrdinalIgnoreCase) &&
                item.Label.Contains("مقاس", StringComparison.OrdinalIgnoreCase));
            if (sizeStockSpec is not null &&
                TryGetSizeStock(sizeStockSpec.Value, request.SelectedSize.Trim(), out var currentSizeStock) &&
                currentSizeStock >= request.Quantity)
            {
                var sizeStocks = JsonSerializer.Deserialize<Dictionary<string, int>>(sizeStockSpec.Value)
                    ?? new Dictionary<string, int>();
                sizeStocks[request.SelectedSize.Trim()] = currentSizeStock - request.Quantity;
                sizeStockSpec.Value = JsonSerializer.Serialize(sizeStocks);
            }
        }

        await _unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _cache.Set(duplicateKey, true, TimeSpan.FromMinutes(1));

        return new GuestOrderResponseDto
        {
            OrderId = order.Id,
            TrackingCode = $"ECO-{order.Id:D6}"
        };
    }

    private static bool TryGetSizeStock(string json, string size, out int stock)
    {
        stock = 0;
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty(size, out var value) ||
                !value.TryGetInt32(out stock))
                return false;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
