using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserByIdAsync(int id);
        Task<List<UserResponseDTO>> GetUsersByRoleAsync(int roleId);
        Task<DashboardCountDTO> GetDashboardCountAsync();
        Task<UserResponseDTO?> UpdateProfileAsync(int id, UpdateProfileDTO dto);
        Task<UserResponseDTO?> CreateUserAsync(CreateUserDTO dto);
        Task<UserResponseDTO?> UpdateUserAsync(int id, UpdateUserDTO dto);
        Task<bool> DeleteUserAsync(int id);
    }
}
