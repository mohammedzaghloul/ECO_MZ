using System.ComponentModel.DataAnnotations;

namespace ECO.BLL.DTO.OrderDtos;

public class GuestOrderRequestDto
{
    [Range(1, int.MaxValue)]
    public int LandingPageId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 20)]
    public int Quantity { get; set; } = 1;

    [StringLength(30)]
    public string? SelectedSize { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^01[0125][0-9]{8}$")]
    public string Phone { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int GovernorateId { get; set; }

    [Range(1, int.MaxValue)]
    public int CityId { get; set; }

    [Required, StringLength(300, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class GuestOrderResponseDto
{
    public int OrderId { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
}
