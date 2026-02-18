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
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Item)
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
