using ECO.DAL.Data;
using ECO.DAL.Entities;
using ECO.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECO.DAL.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        private readonly AppDbContext _dbContext;

        public AddressRepository(AppDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Address?> GetAddressByUserIdAsync(string userId)
        {
            return await _dbContext.Addresses
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
        }

        public async Task<Address> UpdateAddressAsync(
            string userId,
            Address address)
        {
            var existingAddress =
                await GetAddressByUserIdAsync(userId);

            if (existingAddress == null)
            {
                // First address for this user — create it instead of failing.
                address.ApplicationUserId = userId;
                await _dbContext.Addresses.AddAsync(address);
                return address;
            }

            existingAddress.FirstName = address.FirstName;
            existingAddress.LastName = address.LastName;
            existingAddress.Street = address.Street;
            existingAddress.City = address.City;
            existingAddress.State = address.State;
            existingAddress.ZipCode = address.ZipCode;
            existingAddress.Country = address.Country;

            return existingAddress;
        }
    }
}