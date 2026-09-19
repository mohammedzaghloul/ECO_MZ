namespace ECO.BLL.DTO.Order;

public class DeliveryMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string DeliveryTime { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
}
