using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class InventoryRepo 
    {
        private readonly CentralKitchenDBContext _context;

        public InventoryRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            // Sử dụng Include để lấy thêm thông tin Item và Location cho rõ ràng
            return await _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Location)
                .ToListAsync();
        }

        public async Task<Inventory> GetByIdAsync(int id)
        {
            return await _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Entry(inventory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
