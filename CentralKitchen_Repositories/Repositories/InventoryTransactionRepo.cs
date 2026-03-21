using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
  

    public class InventoryTransactionRepo 
    {
        private readonly CentralKitchenDBContext _context;

        public InventoryTransactionRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(int? userId = null)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Item)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Location)
                .AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(t => t.Inventory.ManagedBy == userId.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt) // Mới nhất lên đầu
                .ToListAsync();
        }
        public async Task<InventoryTransaction> GetByIdAsync(int id)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Item)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Location)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
