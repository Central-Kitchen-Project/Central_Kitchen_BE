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

        public async Task<CreateOrderResultDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return new CreateOrderResultDTO { Success = false, Message = "Danh sách sản phẩm không được để trống." };

            // 1. Lấy danh sách ItemIds
            var itemIds = dto.Items.Select(i => i.ItemId).ToList();

            // 2. Kiểm tra tồn kho
            var inventory = await _orderRepo.GetInventoryByItemIdsAsync(itemIds);
            var itemNames = await _orderRepo.GetItemNamesByIdsAsync(itemIds);

            var insufficientItems = new List<InsufficientItemDTO>();
            foreach (var item in dto.Items)
            {
                var available = inventory.ContainsKey(item.ItemId) ? inventory[item.ItemId] : 0;
                if (available < item.Quantity)
                {
                    insufficientItems.Add(new InsufficientItemDTO
                    {
                        ItemId = item.ItemId,
                        ItemName = itemNames.ContainsKey(item.ItemId) ? itemNames[item.ItemId] : "Unknown",
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = available,
                        ShortageQuantity = item.Quantity - available
                    });
                }
            }

            // 3. Nếu thiếu hàng → trả về danh sách thiếu
            if (insufficientItems.Count > 0)
            {
                return new CreateOrderResultDTO
                {
                    Success = false,
                    Message = "Không đủ tồn kho để tạo đơn hàng.",
                    InsufficientItems = insufficientItems
                };
            }

            // 4. Đủ hàng → tạo order
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
            return new CreateOrderResultDTO
            {
                Success = true,
                Message = "Tạo đơn hàng thành công.",
                Order = createdOrder != null ? MapToResponseDTO(createdOrder) : null
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
                return false;

            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return false;

            order.Status = dto.Status;
            var updated = await _orderRepo.UpdateOrderAsync(order);

            // Khi đơn hàng hoàn tất → cập nhật tồn kho
            if (updated && dto.Status == "Completed")
            {
                await ProcessInventoryOnCompleted(order);
            }

            return updated;
        }

        /// <summary>
        /// Xử lý tồn kho khi đơn hàng hoàn tất
        /// - Franchise Store Staff tạo order: CỘNG kho franchise, TRỪ kho supplier
        /// - Supply Coordinator tạo order: TRỪ kho supplier, CỘNG kho franchise  
        /// </summary>
        private async Task ProcessInventoryOnCompleted(Order order)
        {
            var roleName = await _orderRepo.GetUserRoleNameAsync(order.UserId);
            if (string.IsNullOrEmpty(roleName)) return;

            // Tìm userId của bên còn lại
            int franchiseUserId = 0;
            int supplierUserId = 0;

            if (roleName == "Franchise Store Staff")
            {
                franchiseUserId = order.UserId;
                // Tìm supplier đầu tiên
                var suppliers = await _orderRepo.GetUserIdsByRoleAsync("Supply Coordinator");
                if (suppliers.Count > 0) supplierUserId = suppliers[0];
            }
            else if (roleName == "Supply Coordinator")
            {
                supplierUserId = order.UserId;
                // Tìm franchise đầu tiên
                var franchises = await _orderRepo.GetUserIdsByRoleAsync("Franchise Store Staff");
                if (franchises.Count > 0) franchiseUserId = franchises[0];
            }
            else return;

            foreach (var line in order.OrderLines)
            {
                var quantity = line.Quantity ?? 0;
                if (quantity <= 0) continue;

                // CỘNG kho franchise (nhận hàng)
                if (franchiseUserId > 0)
                {
                    await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, franchiseUserId, quantity);
                    await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, franchiseUserId, "order_in", quantity, order.Id);
                }

                // TRỪ kho supplier (xuất hàng)
                if (supplierUserId > 0)
                {
                    await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, supplierUserId, -quantity);
                    await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, supplierUserId, "order_out", quantity, order.Id);
                }
            }
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
                    Name = ol.Item?.ItemName ?? "",
                    Type = ol.Item?.ItemType ?? "",
                    Category = ol.Item?.Category ?? "",
                    Description = ol.Item?.Description ?? "",
                    Price = ol.Item?.Price,
                    Quantity = ol.Quantity,
                    Ingredients = ol.Item?.RecipeFinishedItems?.Select(r => new IngredientDTO
                    {
                        Name = r.IngredientItem?.ItemName ?? "",
                        Qty = r.Quantity,
                        Unit = r.IngredientItem?.Unit ?? ""
                    }).ToList() ?? new List<IngredientDTO>()
                }).ToList()
            };
        }
    }
}
