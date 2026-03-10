using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepo _userRepo;

        public UserService(UserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return users.Select(MapToResponseDTO).ToList();
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            return user != null ? MapToResponseDTO(user) : null;
        }

        public async Task<List<UserResponseDTO>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _userRepo.GetUsersByRoleAsync(roleId);
            return users.Select(MapToResponseDTO).ToList();
        }

        public async Task<DashboardCountDTO> GetDashboardCountAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return new DashboardCountDTO
            {
                TotalUsers = users.Count,
                RoleCounts = users
                    .GroupBy(u => new { u.RoleId, RoleName = u.Role?.RoleName ?? "" })
                    .Select(g => new RoleCountDTO
                    {
                        RoleId = g.Key.RoleId,
                        RoleName = g.Key.RoleName,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.RoleId)
                    .ToList()
            };
        }

        public async Task<UserResponseDTO?> UpdateProfileAsync(int id, UpdateProfileDTO dto)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return null;

            // Kiểm tra trùng username
            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                if (await _userRepo.IsUsernameExistsAsync(dto.Username, id))
                    return null;
                user.Username = dto.Username;
            }

            // Kiểm tra trùng email
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userRepo.IsEmailExistsAsync(dto.Email, id))
                    return null;
                user.Email = dto.Email;
            }

            await _userRepo.UpdateUserAsync(user);

            // Reload
            var updated = await _userRepo.GetUserByIdAsync(id);
            return updated != null ? MapToResponseDTO(updated) : null;
        }

        private UserResponseDTO MapToResponseDTO(CentralKitchen_Repositories.Models.User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "",
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDTO?> CreateUserAsync(CreateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Email))
                return null;

            if (await _userRepo.IsUsernameExistsAsync(dto.Username, 0))
                return null;
            if (await _userRepo.IsEmailExistsAsync(dto.Email, 0))
                return null;

            var user = new CentralKitchen_Repositories.Models.User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                RoleId = dto.RoleId,
                CreatedAt = DateTime.Now
            };

            var created = await _userRepo.CreateUserAsync(user);
            return created != null ? MapToResponseDTO(created) : null;
        }

        public async Task<UserResponseDTO?> UpdateUserAsync(int id, UpdateUserDTO dto)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                if (await _userRepo.IsUsernameExistsAsync(dto.Username, id))
                    return null;
                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userRepo.IsEmailExistsAsync(dto.Email, id))
                    return null;
                user.Email = dto.Email;
            }

            if (dto.RoleId.HasValue)
                user.RoleId = dto.RoleId.Value;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepo.UpdateUserAsync(user);
            var updated = await _userRepo.GetUserByIdAsync(id);
            return updated != null ? MapToResponseDTO(updated) : null;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepo.DeleteUserAsync(id);
        }
    }
}
