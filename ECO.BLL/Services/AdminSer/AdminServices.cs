using AutoMapper;
using ECO.BLL.DTO.AdminDtos;
using ECO.BLL.DTO.OrderDtos;
using ECO.DAL.Entities;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using Microsoft.AspNetCore.Identity;
using ECO.BLL.Services.Notifications;
using Microsoft.Extensions.Logging;

namespace ECO.BLL.Services.AdminSer
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<AdminOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<OrderToReturnDto>> GetAllAsync(
            string? status,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var specification = new OrderSpecification(status, search, fromDate, toDate, pageNumber, pageSize);
            var orders = await _unitOfWork.Repository<Order>().ListAsync(specification, cancellationToken);
            return _mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders);
        }

        public Task<int> GetCountAsync(
            string? status,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            // CountAsync ignores paging, so the same specification yields the full filtered count.
            return _unitOfWork.Repository<Order>().CountAsync(
                new OrderSpecification(status, search, fromDate, toDate, 1, int.MaxValue), cancellationToken);
        }

        public async Task<OrderToReturnDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Repository<Order>().FirstOrDefaultAsync(
                new OrderSpecification(id, byId: true), cancellationToken);
            var result = _mapper.Map<OrderToReturnDto?>(order);
            if (result is not null)
            {
                var user = await _userManager.FindByEmailAsync(result.BuyerEmail);
                result.BuyerPhone = user?.PhoneNumber ?? string.Empty;
            }
            return result;
        }

        public async Task<bool> UpdateStatusAsync(int id, Status status, CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id, cancellationToken);
            if (order is null)
                return false;

            var previousStatus = order.Status;
            if (previousStatus == status)
                return true;

            order.Status = status;
            await _unitOfWork.Repository<Order>().UpdateAsync(order, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            try
            {
                await _notificationService.CreateAsync(
                    order.BuyerEmail,
                    "Order status updated",
                    $"Your order #{order.Id} is now {GetStatusMessage(status)}.",
                    "Order",
                    order.Id,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create status notification for order {OrderId}", order.Id);
            }

            return true;
        }

        private static string GetStatusMessage(Status status) => status switch
        {
            Status.PaymentRecevied => "payment received",
            Status.PaymentFaild => "payment failed",
            Status.Shipped => "shipped",
            Status.Delivered => "delivered",
            _ => "pending"
        };
    }

    public class CustomerAdminService : ICustomerAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public CustomerAdminService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(string? search, string? role, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var rows = await _unitOfWork.AdminRepository.GetCustomersAsync(
                search, role, (pageNumber - 1) * pageSize, pageSize, cancellationToken);

            var result = new List<CustomerListItemDto>();
            foreach (var row in rows)
            {
                var roles = await _userManager.GetRolesAsync(new ApplicationUser { Id = row.Id });
                result.Add(new CustomerListItemDto
                {
                    Id = row.Id,
                    DisplayName = row.DisplayName,
                    Email = row.Email,
                    OrdersCount = row.OrdersCount,
                    TotalSpent = row.TotalSpent,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public Task<int> GetCustomersCountAsync(string? search, string? role, CancellationToken cancellationToken = default)
            => _unitOfWork.AdminRepository.GetCustomersCountAsync(search, role, cancellationToken);

        public async Task<CustomerDetailDto?> GetCustomerAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return null;

            var orders = await _unitOfWork.Repository<Order>().ListAsync(
                new OrderSpecification(user.Email ?? string.Empty), cancellationToken);

            return new CustomerDetailDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                OrdersCount = orders.Count,
                TotalSpent = orders.Sum(order => order.GetTotal()),
                Orders = _mapper.Map<List<OrderToReturnDto>>(orders)
            };
        }

        public async Task<bool> SetAdminRoleAsync(string userId, bool addToRole, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var isInRole = await _userManager.IsInRoleAsync(user, "Admin");
            if (addToRole && !isInRole)
                await _userManager.AddToRoleAsync(user, "Admin");
            else if (!addToRole && isInRole)
                await _userManager.RemoveFromRoleAsync(user, "Admin");

            return true;
        }

        public async Task<bool> DeleteCustomerAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || await _userManager.IsInRoleAsync(user, "Admin"))
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }

    public class AdminDiscountService : IAdminDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminDiscountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<DiscountDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var discounts = await _unitOfWork.Repository<Discount>()
                .ListAsync(cancellationToken: cancellationToken);
            return discounts.Select(ToDto).ToList();
        }

        public async Task<DiscountDto> SaveAsync(SaveDiscountDto dto, CancellationToken cancellationToken = default)
        {
            var code = dto.Code.Trim().ToUpperInvariant();

            var conflict = await _unitOfWork.Repository<Discount>().FirstOrDefaultAsync(
                new DiscountSpecification(code), cancellationToken);
            if (conflict is not null && conflict.Id != dto.Id)
                throw new InvalidOperationException($"Coupon code '{code}' already exists.");

            Discount discount;
            if (dto.Id > 0)
            {
                discount = await _unitOfWork.Repository<Discount>().GetByIdAsync(dto.Id, cancellationToken);
                if (discount is null)
                    throw new InvalidOperationException($"Discount with id {dto.Id} was not found.");
            }
            else
            {
                discount = new Discount();
            }

            discount.Code = code;
            discount.IsPercentage = dto.IsPercentage;
            discount.Value = dto.Value;
            discount.IsActive = dto.IsActive;
            discount.ExpiryDate = dto.ExpiryDate;
            discount.MaxUses = dto.MaxUses;

            if (dto.Id > 0)
            {
                await _unitOfWork.Repository<Discount>().UpdateAsync(discount, cancellationToken);
            }
            else
            {
                await _unitOfWork.Repository<Discount>().AddAsync(discount, cancellationToken);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return ToDto(discount);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var discount = await _unitOfWork.Repository<Discount>().GetByIdAsync(id, cancellationToken);
            if (discount is null)
                return false;

            await _unitOfWork.Repository<Discount>().DeleteAsync(id, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        private static DiscountDto ToDto(Discount discount)
        {
            return new DiscountDto
            {
                Id = discount.Id,
                Code = discount.Code,
                IsPercentage = discount.IsPercentage,
                Value = discount.Value,
                IsActive = discount.IsActive,
                ExpiryDate = discount.ExpiryDate,
                MaxUses = discount.MaxUses,
                UsedCount = discount.UsedCount
            };
        }
    }
}
