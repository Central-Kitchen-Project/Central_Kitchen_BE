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

		// Danh sách tr?ng thái h?p l?
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
				return new CreateOrderResultDTO { Success = false, Message = "Danh sách s?n ph?m không ???c ?? tr?ng." };

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
				Message = "T?o ??n hàng thành công.",
				Order = createdOrder != null ? MapToResponseDTO(createdOrder) : null
			};
		}

		public async Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto)
		{
			if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
				return false;

			var order = await _orderRepo.GetOrderByIdAsync(id);
			if (order == null) return false;

			// Prevent re-processing if order is already in the target status
			if (order.Status == dto.Status) return false;

			// Save order line data before status update for inventory processing
			var orderUserId = order.UserId;
			var orderLines = order.OrderLines.Select(ol => new { ol.ItemId, ol.Quantity }).ToList();
			var lineItems = orderLines.Select(ol => (ol.ItemId, ol.Quantity ?? 0m)).ToList();

			// When accepting order (Approved), validate and deduct from the approving supply coordinator
			if (dto.Status == "Approved")
			{
				if (!dto.ApprovedBy.HasValue || dto.ApprovedBy.Value <= 0)
					return false;

				var approverRole = await _orderRepo.GetUserRoleNameAsync(dto.ApprovedBy.Value);
				if (approverRole != "SupplyCoordinator")
					return false;

				var itemIds = order.OrderLines.Select(ol => ol.ItemId).Distinct().ToList();
				var approverInventory = await _orderRepo.GetInventoryByUserAndItemsAsync(itemIds, dto.ApprovedBy.Value);

				// Check that this supply coordinator has enough inventory
				var totalNeededPerItem = order.OrderLines
					.GroupBy(ol => ol.ItemId)
					.ToDictionary(g => g.Key, g => g.Sum(ol => ol.Quantity ?? 0));

				foreach (var kvp in totalNeededPerItem)
				{
					if (kvp.Value <= 0) continue;

					var available = approverInventory.ContainsKey(kvp.Key) ? approverInventory[kvp.Key] : 0;
					if (available < kvp.Value)
						return false;
				}

				// Deduct from the approving supply coordinator's inventory immediately
				order.ApprovedBy = dto.ApprovedBy.Value;
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					foreach (var line in lineItems)
					{
						if (line.Item2 <= 0) continue;
						await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, dto.ApprovedBy.Value, -line.Item2);
						await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, dto.ApprovedBy.Value, "order_out", line.Item2, id);
					}
				}

				return updated;
			}

			// When order is completed ? add inventory to the franchise store
			if (dto.Status == "Completed")
			{
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					await AddInventoryToFranchise(orderUserId, lineItems, id);
				}

				return updated;
			}

			// When order is cancelled after approval ? restore inventory to the supply coordinator
			if (dto.Status == "Cancelled" && order.ApprovedBy.HasValue)
			{
				var supplierId = order.ApprovedBy.Value;
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					foreach (var line in lineItems)
					{
						if (line.Item2 <= 0) continue;
						await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, supplierId, line.Item2);
						await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, supplierId, "order_cancel_restore", line.Item2, id);
					}
				}

				return updated;
			}

			// For other status transitions (Processing, Cancelled from Pending, etc.)
			order.Status = dto.Status;
			return await _orderRepo.UpdateOrderAsync(order);
		}

		/// <summary>
		/// Add inventory to the franchise store when order is completed.
		/// The supply coordinator's inventory was already deducted at approval time.
		/// </summary>
		private async Task AddInventoryToFranchise(int orderUserId, List<(int ItemId, decimal Quantity)> orderLines, int orderId)
		{
			var roleName = await _orderRepo.GetUserRoleNameAsync(orderUserId);
			if (string.IsNullOrEmpty(roleName)) return;

			int franchiseUserId = 0;

			if (roleName == "FranchiseStore")
			{
				franchiseUserId = orderUserId;
			}
			else if (roleName == "SupplyCoordinator")
			{
				var franchises = await _orderRepo.GetUserIdsByRoleAsync("FranchiseStore");
				if (franchises.Count > 0) franchiseUserId = franchises[0];
			}
			else return;

			if (franchiseUserId <= 0) return;

			foreach (var line in orderLines)
			{
				if (line.Quantity <= 0) continue;
				await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, franchiseUserId, line.Quantity);
				await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, franchiseUserId, "order_in", line.Quantity, orderId);
			}
		}

		public async Task<bool> DeleteOrderAsync(int id)
		{
			var order = await _orderRepo.GetOrderByIdAsync(id);
			if (order == null) return false;

			// Ch? cho phép xoá ??n hàng có tr?ng thái "Pending"
			if (order.Status != "Pending")
				return false;

			return await _orderRepo.DeleteOrderAsync(id);
		}

		// ===== Helper: Map Entity ? DTO =====
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
