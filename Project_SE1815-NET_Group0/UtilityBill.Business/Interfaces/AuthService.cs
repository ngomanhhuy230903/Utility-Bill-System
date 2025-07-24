// File: UtilityBill.Business/Services/AuthService.cs

using Microsoft.AspNetCore.Identity; // <-- THÊM USING NÀY
using UtilityBill.Business.DTOs;
using UtilityBill.Business.Interfaces;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace UtilityBill.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        // BƯỚC 1: INJECT IPasswordHasher VÀO ĐÂY
        private readonly IPasswordHasher<User> _passwordHasher;

        // Sửa lại constructor để nhận thêm IPasswordHasher
        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> LoginAsync(LoginDto loginDto)
        {
            var user = await _unitOfWork.UserRepository.GetByUsernameAsync(loginDto.UserName);
            if (user == null)
            {
                return null; // Không tìm thấy user
            }

            // BƯỚC 2: DÙNG _passwordHasher ĐỂ KIỂM TRA MẬT KHẨU
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null; // Sai mật khẩu
            }

            // Nếu mật khẩu cũ cần được cập nhật lại với thuật toán hash mới hơn (tính năng của Identity)
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // Tùy chọn: bạn có thể cập nhật lại hash ở đây
            }

            return user; // Đăng nhập thành công
        }

        public async Task<User?> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _unitOfWork.UserRepository.GetByUsernameAsync(registerDto.UserName);
            if (existingUser != null)
            {
                return null; // Username đã tồn tại
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                PhoneNumber = registerDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // BƯỚC 3: DÙNG _passwordHasher ĐỂ BĂM MẬT KHẨU KHI ĐĂNG KÝ
            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }
    }
}