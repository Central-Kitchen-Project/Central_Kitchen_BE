using CentralKitchen_Repositories.Models;
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
    public class RoleService : IRoleService
    {
        private readonly RoleRepo _roleRepo;

        public RoleService(RoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<List<RoleResponseDTO>> GetAllRolesAsync()
        {
            var roles = await _roleRepo.GetAllRolesAsync();
            return roles.Select(MapToResponseDTO).ToList();
        }

        public async Task<RoleResponseDTO?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepo.GetRoleByIdAsync(id);
            return role != null ? MapToResponseDTO(role) : null;
        }

        public async Task<RoleResponseDTO?> CreateRoleAsync(CreateRoleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return null;

            if (await _roleRepo.IsRoleNameExistsAsync(dto.RoleName, 0))
                return null;

            var role = new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description
            };

            var created = await _roleRepo.CreateRoleAsync(role);
            return created != null ? MapToResponseDTO(created) : null;
        }

        public async Task<RoleResponseDTO?> UpdateRoleAsync(int id, UpdateRoleDTO dto)
        {
            var role = await _roleRepo.GetRoleByIdAsync(id);
            if (role == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.RoleName) && dto.RoleName != role.RoleName)
            {
                if (await _roleRepo.IsRoleNameExistsAsync(dto.RoleName, id))
                    return null;
                role.RoleName = dto.RoleName;
            }

            if (dto.Description != null)
                role.Description = dto.Description;

            await _roleRepo.UpdateRoleAsync(role);
            var updated = await _roleRepo.GetRoleByIdAsync(id);
            return updated != null ? MapToResponseDTO(updated) : null;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _roleRepo.DeleteRoleAsync(id);
        }

        private RoleResponseDTO MapToResponseDTO(Role role)
        {
            return new RoleResponseDTO
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                UserCount = role.Users?.Count ?? 0
            };
        }
    }
}
