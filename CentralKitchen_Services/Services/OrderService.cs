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
    public class OrderService : IOrderService
    {
        private readonly OrderRepo _orderRepo;

        // Danh sách trạng thái hợp lệ
        private static readonly List<string> ValidStatuses = new List<string>
        {
            "Pending", "Approved", "Processing", "Completed", "Cancelled"
        };

        public OrderService(OrderRepo orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<List<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            return orders.Select(MapToResponseDTO).ToList();
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return null;
            return MapToResponseDTO(order);
        }

        public async Task<OrderResponseDTO?> CreateOrderAsync(CreateOrderDTO dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return null;

            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderLines = dto.Items.Select(item => new OrderLine
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var createdOrder = await _orderRepo.CreateOrderAsync(order);
            return createdOrder != null ? MapToResponseDTO(createdOrder) : null;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
                return false;

            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return false;

            order.Status = dto.Status;
            return await _orderRepo.UpdateOrderAsync(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return false;

            // Chỉ cho phép xoá đơn hàng có trạng thái "Pending"
            if (order.Status != "Pending")
                return false;

            return await _orderRepo.DeleteOrderAsync(id);
        }

        // ===== Helper: Map Entity → DTO =====
        private OrderResponseDTO MapToResponseDTO(Order order)
        {
            return new OrderResponseDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                Username = order.User?.Username ?? "",
                OrderDate = order.OrderDate,
                Status = order.Status,
                OrderLines = order.OrderLines.Select(ol => new OrderLineResponseDTO
                {
                    Id = ol.Id,
                    ItemId = ol.ItemId,
                    ItemName = ol.Item?.ItemName ?? "",
                    Quantity = ol.Quantity,
                    Unit = ol.Item?.Unit ?? ""
                }).ToList()
            };
        }
    }
}
