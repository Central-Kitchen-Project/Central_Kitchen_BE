using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class UserRepo
    {
        private readonly CentralKitchenDBContext _context;

        public UserRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // After creating a new user, check if role is FranchiseStore or SupplyCoordinator
            // and create inventory records for all items if inventory is empty
            var createdUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (createdUser?.Role?.RoleName == "FranchiseStore" || createdUser?.Role?.RoleName == "SupplyCoordinator")
            {
                var hasInventory = await _context.Inventories
                    .AnyAsync(inv => inv.ManagedBy == createdUser.Id);

                if (!hasInventory)
                {
                    var allItems = await _context.Items.ToListAsync();
                    var defaultLocation = await _context.Locations.FirstOrDefaultAsync();
                    var locationId = defaultLocation?.Id ?? 1;

                    foreach (var item in allItems)
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            ItemId = item.Id,
                            LocationId = locationId,
                            Quantity = 0,
                            ManagedBy = createdUser.Id
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return await GetUserByIdAsync(user.Id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Get user's orders
            var orderIds = await _context.Orders
                .Where(o => o.UserId == id)
                .Select(o => o.Id)
                .ToListAsync();

            // Get user's material requests (both direct and through orders)
            var materialRequestIds = await _context.MaterialRequests
                .Where(mr => mr.RequestedByUserId == id || orderIds.Contains(mr.OrderId))
                .Select(mr => mr.Id)
                .ToListAsync();

            // Delete material request lines
            var materialRequestLines = await _context.MaterialRequestLines
                .Where(mrl => materialRequestIds.Contains(mrl.MaterialRequestId))
                .ToListAsync();
            _context.MaterialRequestLines.RemoveRange(materialRequestLines);

            // Delete material requests
            var materialRequests = await _context.MaterialRequests
                .Where(mr => materialRequestIds.Contains(mr.Id))
                .ToListAsync();
            _context.MaterialRequests.RemoveRange(materialRequests);

            // Delete order lines
            var orderLines = await _context.OrderLines
                .Where(ol => orderIds.Contains(ol.OrderId))
                .ToListAsync();
            _context.OrderLines.RemoveRange(orderLines);

            // Delete quality feedbacks (by user or by user's orders)
            var qualityFeedbacks = await _context.QualityFeedbacks
                .Where(qf => qf.UserId == id || (qf.OrderId.HasValue && orderIds.Contains(qf.OrderId.Value)))
                .ToListAsync();
            _context.QualityFeedbacks.RemoveRange(qualityFeedbacks);

            // Delete orders
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .ToListAsync();
            _context.Orders.RemoveRange(orders);

            // Get user's inventories
            var inventoryIds = await _context.Inventories
                .Where(inv => inv.ManagedBy == id)
                .Select(inv => inv.Id)
                .ToListAsync();

            // Delete inventory transactions
            var inventoryTransactions = await _context.InventoryTransactions
                .Where(it => inventoryIds.Contains(it.InventoryId))
                .ToListAsync();
            _context.InventoryTransactions.RemoveRange(inventoryTransactions);

            // Delete inventories
            var inventories = await _context.Inventories
                .Where(inv => inv.ManagedBy == id)
                .ToListAsync();
            _context.Inventories.RemoveRange(inventories);

            // Delete user
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int excludeUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username && u.Id != excludeUserId);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int excludeUserId)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }
    }
}
