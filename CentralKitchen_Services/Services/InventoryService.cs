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
    public class InventoryService : IInventoryService
    {
        private readonly InventoryRepo _repo;

        public InventoryService(InventoryRepo repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<InventoryDTO>> GetAllInventories()
        {
            var inventories = await _repo.GetAllAsync();

            // Chuyển đổi thủ công từ Entity sang DTO
            return inventories.Select(inv => new InventoryDTO
            {
                Id = inv.Id,
                Quantity = inv.Quantity,
                Item = inv.Item == null ? null : new ItemDTO
                {
                    Id = inv.Item.Id,
                    ItemName = inv.Item.ItemName,
                    Unit = inv.Item.Unit,
                    ItemType = inv.Item.ItemType,
                    Category = inv.Item.Category,
                    Price = inv.Item.Price,
                    Description = inv.Item.Description
                },
                Location = inv.Location == null ? null : new LocationDTO
                {
                    Id = inv.Location.Id,
                    LocationName = inv.Location.LocationName,
                    Address = inv.Location.Address
                }
            });
        }

        public async Task<InventoryDTO> GetInventoryById(int id)
        {
            // Lấy dữ liệu từ Repository (đã bao gồm Include Item và Location)
            var inv = await _repo.GetByIdAsync(id);

            // Kiểm tra nếu không tìm thấy bản ghi
            if (inv == null) return null;

            // Trả về DTO với đầy đủ thông tin được gán trực tiếp
            return new InventoryDTO
            {
                Id = inv.Id,
                Quantity = inv.Quantity,

                // Gán dữ liệu cho ItemDTO
                Item = inv.Item == null ? null : new ItemDTO
                {
                    Id = inv.Item.Id,
                    ItemName = inv.Item.ItemName,
                    Unit = inv.Item.Unit,
                    ItemType = inv.Item.ItemType,
                    Category = inv.Item.Category,
                    Price = inv.Item.Price,
                    Description = inv.Item.Description
                },

                // Gán dữ liệu cho LocationDTO
                Location = inv.Location == null ? null : new LocationDTO
                {
                    Id = inv.Location.Id,
                    LocationName = inv.Location.LocationName,
                    Address = inv.Location.Address
                }
            };
        }

        public async Task<bool> UpdateStock(int id, decimal newQuantity)
        {
            var inventory = await _repo.GetByIdAsync(id);
            if (inventory == null) return false;

            // Logic nghiệp vụ: Cập nhật số lượng
            inventory.Quantity = newQuantity;

            await _repo.UpdateAsync(inventory);
            return true;
        }

    }
}
