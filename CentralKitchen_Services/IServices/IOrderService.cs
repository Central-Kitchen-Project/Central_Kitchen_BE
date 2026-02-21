using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IOrderService
    {
        Task<List<OrderResponseDTO>> GetAllOrdersAsync();
        Task<OrderResponseDTO?> GetOrderByIdAsync(int id);
        Task<CreateOrderResultDTO> CreateOrderAsync(CreateOrderDTO dto);
        Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
