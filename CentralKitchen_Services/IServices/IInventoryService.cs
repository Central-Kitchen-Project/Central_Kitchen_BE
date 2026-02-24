using CentralKitchen_Repositories.Models;
using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDTO>> GetAllInventories();
        Task<InventoryDTO> GetInventoryById(int id);
        Task<bool> UpdateStock(int id, decimal newQuantity);
    }


}
