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
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly InventoryTransactionRepo _repo;

        public InventoryTransactionService(InventoryTransactionRepo repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<InventoryTransactionDTO>> GetTransactionHistory()
        {
            var transactions = await _repo.GetAllTransactionsAsync();

            return transactions.Select(t => new InventoryTransactionDTO
            {
                Id = t.Id,
                TxType = t.TxType,
                Quantity = t.Quantity,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                CreatedAt = t.CreatedAt,
                // Lấy thông tin từ bảng Item thông qua Inventory
                ItemName = t.Inventory?.Item?.ItemName,
                Unit = t.Inventory?.Item?.Unit,
                // Lấy thông tin từ bảng Location thông qua Inventory
                LocationName = t.Inventory?.Location?.LocationName
            });
        }
        public async Task<InventoryTransactionDTO> GetTransactionById(int id)
        {
            var t = await _repo.GetByIdAsync(id);

            if (t == null) return null;

            return new InventoryTransactionDTO
            {
                Id = t.Id,
                TxType = t.TxType,
                Quantity = t.Quantity,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                CreatedAt = t.CreatedAt,
                InventoryId = t.InventoryId,

                // Gán trực tiếp dữ liệu từ các bảng liên quan (Items & Locations)
                ItemName = t.Inventory?.Item?.ItemName,
                Unit = t.Inventory?.Item?.Unit,
                LocationName = t.Inventory?.Location?.LocationName
            };
        }
    }
    }
