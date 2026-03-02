using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class OrderRepo
    {
        private readonly CentralKitchenDBContext _context;

        public OrderRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Item)
                        .ThenInclude(i => i.RecipeFinishedItems)
                            .ThenInclude(r => r.IngredientItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Item)
                        .ThenInclude(i => i.RecipeFinishedItems)
                            .ThenInclude(r => r.IngredientItem)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            // Reload with includes
            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Lấy tổng tồn kho của các item (gộp tất cả location)
        /// </summary>
        public async Task<Dictionary<int, decimal>> GetInventoryByItemIdsAsync(List<int> itemIds)
        {
            return await _context.Inventories
                .Where(inv => itemIds.Contains(inv.ItemId))
                .GroupBy(inv => inv.ItemId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(inv => inv.Quantity ?? 0)
                );
        }

        /// <summary>
        /// Lấy tên item theo danh sách IDs
        /// </summary>
        public async Task<Dictionary<int, string>> GetItemNamesByIdsAsync(List<int> itemIds)
        {
            return await _context.Items
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.ItemName);
        }

        /// <summary>
        /// Lấy role name của user theo userId
        /// </summary>
        public async Task<string?> GetUserRoleNameAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Role?.RoleName;
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho (cộng hoặc trừ)
        /// </summary>
        public async Task<bool> UpdateInventoryQuantityAsync(int itemId, int locationId, decimal quantityChange)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.LocationId == locationId);

            if (inventory == null)
            {
                // Tạo mới nếu chưa có record inventory cho item này tại location
                inventory = new Inventory
                {
                    ItemId = itemId,
                    LocationId = locationId,
                    Quantity = quantityChange > 0 ? quantityChange : 0
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity = (inventory.Quantity ?? 0) + quantityChange;
                if (inventory.Quantity < 0) inventory.Quantity = 0;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Ghi log giao dịch kho
        /// </summary>
        public async Task CreateInventoryTransactionAsync(int itemId, int locationId, string txType, decimal quantity, int orderId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.LocationId == locationId);

            if (inventory != null)
            {
                var transaction = new InventoryTransaction
                {
                    InventoryId = inventory.Id,
                    TxType = txType,
                    Quantity = quantity,
                    ReferenceType = "order",
                    ReferenceId = orderId,
                    CreatedAt = DateTime.Now
                };
                _context.InventoryTransactions.Add(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return false;

            _context.OrderLines.RemoveRange(order.OrderLines);
            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
