using AutoMapper;
using ECO.BLL.DTO.AddressDtos;
using ECO.BLL.DTO.Order;
using ECO.BLL.Services.Identity;
using ECO.DAL.Entities;
using ECO.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.UserInfo
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper mapper;
        private readonly ICurrentUserService currentUser;
        private readonly UserManager<ApplicationUser> userManager;

        public AddressService( IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.currentUser = currentUser;
            this.userManager = userManager;
        }

        public async Task<AddressDto?> GetUserAddressAsync()
        {
            var address = await _unitOfWork.AddressRepository.GetAddressByUserIdAsync(currentUser.UserId);
            return address == null ? null : mapper.Map<AddressDto>(address);
        }

        public async Task<AddressDto> UpdateAsync(ShippingAddressDto shippingAddressDto)
        {
            var addressAddDto = mapper.Map<AddressAddDto>(shippingAddressDto);
            var address = mapper.Map<Address>(addressAddDto);
            var updatedAddress = await _unitOfWork.AddressRepository.UpdateAddressAsync(currentUser.UserId, address);
            await _unitOfWork.CompleteAsync();
            return mapper.Map<AddressDto>(updatedAddress);
        }

        public async Task<AddressDto> UpdateByEmailAsync(string email, ShippingAddressDto shippingAddressDto)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var addressAddDto = mapper.Map<AddressAddDto>(shippingAddressDto);
            var address = mapper.Map<Address>(addressAddDto);
            var updatedAddress = await _unitOfWork.AddressRepository.UpdateAddressAsync(user.Id, address);
            await _unitOfWork.CompleteAsync();
            return mapper.Map<AddressDto>(updatedAddress);
        }

        }
}

