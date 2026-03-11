using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IRoleService
    {
        Task<List<RoleResponseDTO>> GetAllRolesAsync();
        Task<RoleResponseDTO?> GetRoleByIdAsync(int id);
        Task<RoleResponseDTO?> CreateRoleAsync(CreateRoleDTO dto);
        Task<RoleResponseDTO?> UpdateRoleAsync(int id, UpdateRoleDTO dto);
        Task<bool> DeleteRoleAsync(int id);
    }
}
