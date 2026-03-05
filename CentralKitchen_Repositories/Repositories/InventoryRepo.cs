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

        public async Task<IEnumerable<Inventory>> GetAllAsync(int userId)
        {
            // Lấy tất cả tồn kho mà ManagedBy khớp với userId truyền vào
            return await _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Location)
                .Include(i => i.ManagedByUser) // Liên kết với bảng User qua Navigation Property
                .Where(i => i.ManagedBy == userId) // Lọc theo đúng ID người quản lý
                .ToListAsync();
        }

        public async Task<Inventory> GetByIdAsync(int id, int userId)
        {
            // Lấy chi tiết và đảm bảo bản ghi đó thuộc quyền quản lý của userId
            return await _context.Inventories
                .Include(i => i.Item)
                .Include(i => i.Location)
                .Include(i => i.ManagedByUser)
                .FirstOrDefaultAsync(i => i.Id == id && i.ManagedBy == userId);
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Entry(inventory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
