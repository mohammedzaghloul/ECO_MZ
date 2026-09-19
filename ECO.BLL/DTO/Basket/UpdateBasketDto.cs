using System.Text.Json.Serialization;

namespace ECO.BLL.DTO.Basket;

public class UpdateBasketDto
{
    public string Id { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public List<UpdateBasketItemDto> Items { get; set; } = [];
}

public class UpdateBasketItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    // Temporary backward compatibility: old clients still send "qunatity" (the old misspelling). Remove once no old client remains.
    [JsonPropertyName("qunatity")]
    public int QuantityLegacy
    {
        get => Quantity;
        set => Quantity = value;
    }
}
