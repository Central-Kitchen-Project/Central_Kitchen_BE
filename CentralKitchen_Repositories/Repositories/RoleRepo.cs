using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class RoleRepo
    {
        private readonly CentralKitchenDBContext _context;

        public RoleRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Users)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return await GetRoleByIdAsync(role.Id);
        }

        public async Task<bool> UpdateRoleAsync(Role role)
        {
            _context.Roles.Update(role);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return false;

            // Không cho xoá role nếu còn user đang dùng
            if (role.Users.Count > 0) return false;

            _context.Roles.Remove(role);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsRoleNameExistsAsync(string roleName, int excludeRoleId)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleName == roleName && r.Id != excludeRoleId);
        }
    }
}
