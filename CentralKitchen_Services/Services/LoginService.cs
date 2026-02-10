using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repository;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class LoginService : ILoginService
    {

        private readonly LoginRepo _repository;

        public LoginService(LoginRepo repository)
        {
            _repository = repository;
        }
        public User? Login(string email, string password)
        {
            var user = _repository.GetAccountByEmail(email);
            if (user == null) return null;
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        public async Task<bool> Register(RegisterRequestDTO registerDto)
        {
            if (await _repository.UserExists(registerDto.Username))
            {
                return false;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUser = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                RoleId = registerDto.RoleId,
                CreatedAt = DateTime.Now
            };

            return await _repository.RegisterUser(newUser);
        }

        public async Task<bool> ChangePassword(ChangePasswordDTO changePasswordDto)
        {
            // 1. Tìm user theo Email
            var user = _repository.GetAccountByEmail(changePasswordDto.Email);
            if (user == null) return false;

            // 2. Kiểm tra mật khẩu cũ có đúng không
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid) return false;

            // 3. Hash mật khẩu mới
            string newHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            // 4. Cập nhật vào DB
            return await _repository.UpdatePassword(changePasswordDto.Email, newHash);
        }
    }
}

