namespace ECO.BLL.Constants;

public static class OrderErrorMessages
{
    public static string InvalidQuantity(int min, int max) =>
        $"Quantity must be between {min} and {max}";

    public static string ProductsUnavailableFor(IEnumerable<int> productIds)
    {
        var ids = productIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        return ids.Length == 0
            ? "One or more products are no longer available"
            : $"Products are no longer available: {string.Join(", ", ids)}";
    }

    public static string InsufficientStockFor(IEnumerable<int> productIds)
    {
        var ids = productIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        return ids.Length == 0
            ? "One or more products do not have enough stock available"
            : $"Insufficient stock for products: {string.Join(", ", ids)}";
    }
}
