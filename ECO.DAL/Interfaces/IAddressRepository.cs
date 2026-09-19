using ECO.DAL.Entities;

namespace ECO.DAL.Interfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        Task<Address?> GetAddressByUserIdAsync(string userId);

        Task<Address> UpdateAddressAsync( string userId, Address address);
    }
}