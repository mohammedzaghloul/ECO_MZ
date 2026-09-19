using ECO.BLL.DTO;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.DTO.OrderDtos;
using ECO.DAL.Entities.OrderEntities;

namespace ECO.BLL.Services.AdminSer
{
    public interface IAdminOrderService
    {
        Task<IReadOnlyList<OrderToReturnDto>> GetAllAsync(
            string? status,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<int> GetCountAsync(
            string? status,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);
        Task<OrderToReturnDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(int id, Status status, CancellationToken cancellationToken = default);
    }

    public interface ICustomerAdminService
    {
        Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(string? search, string? role, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<int> GetCustomersCountAsync(string? search, string? role, CancellationToken cancellationToken = default);
        Task<CustomerDetailDto?> GetCustomerAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> SetAdminRoleAsync(string userId, bool addToRole, CancellationToken cancellationToken = default);
        Task<bool> DeleteCustomerAsync(string userId, CancellationToken cancellationToken = default);
    }

    public interface IAdminDiscountService
    {
        Task<IReadOnlyList<DiscountDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DiscountDto> SaveAsync(SaveDiscountDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }

    public interface ILocationCatalogService
    {
        Task<LocationCatalogDto> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<LocationCatalogDto> SaveAsync(SaveLocationCatalogDto dto, CancellationToken cancellationToken = default);
    }
}
