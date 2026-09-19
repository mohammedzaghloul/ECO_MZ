namespace ECO.BLL.Services.Basket;

public sealed class BasketSettings
{
    public bool EnableStockValidation { get; set; }
    public int MaxQuantityPerItem { get; set; } = 100;
}
