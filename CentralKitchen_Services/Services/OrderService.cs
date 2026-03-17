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
			"Pending", "Confirmed", "Approved", "Processing", "Delivery", "Completed", "Cancelled", "Rejected"
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

		public async Task<StatusUpdateResultDTO> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto)
		{
			if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
				return new StatusUpdateResultDTO { Success = false, Message = "Tr?ng thái không h?p l?." };

			var order = await _orderRepo.GetOrderByIdAsync(id);
			if (order == null) return new StatusUpdateResultDTO { Success = false, Message = "Không tìm th?y ??n hàng." };

			// Prevent re-processing if order is already in the target status
			if (order.Status == dto.Status) return new StatusUpdateResultDTO { Success = false, Message = "??n hàng ?ã ? tr?ng thái này." };

			// Save order line data before status update for inventory processing
			var orderUserId = order.UserId;

			var rawLineItems = new List<(int ItemId, decimal Quantity)>();
			foreach (var ol in order.OrderLines)
			{
				var qty = ol.Quantity ?? 0m;
				if (qty <= 0) continue;

				if (ol.Item != null && ol.Item.RecipeFinishedItems != null && ol.Item.RecipeFinishedItems.Any())
				{
					// It's a finished product, convert to ingredients
					foreach (var recipe in ol.Item.RecipeFinishedItems)
					{
						rawLineItems.Add((recipe.IngredientItemId, recipe.Quantity * qty));
					}
				}
				else
				{
					// It's an ingredient or un-reciped item
					rawLineItems.Add((ol.ItemId, qty));
				}
			}

			// Aggregate grouped quantities
			var lineItems = rawLineItems
				.GroupBy(x => x.ItemId)
				.Select(g => (ItemId: g.Key, Quantity: g.Sum(x => x.Quantity)))
				.ToList();

			// When accepting order (Approved), validate and deduct from the approving supply coordinator
			if (dto.Status == "Approved")
			{
				if (!dto.ApprovedBy.HasValue || dto.ApprovedBy.Value <= 0)
					return new StatusUpdateResultDTO { Success = false, Message = "Ng??i duy?t không h?p l?." };

				var approverRole = await _orderRepo.GetUserRoleNameAsync(dto.ApprovedBy.Value);
				if (approverRole != "SupplyCoordinator")
					return new StatusUpdateResultDTO { Success = false, Message = "Ch? Supply Coordinator m?i ???c duy?t ??n hàng." };

				var itemIds = lineItems.Select(l => l.ItemId).Distinct().ToList();
				var approverInventory = await _orderRepo.GetInventoryByUserAndItemsAsync(itemIds, dto.ApprovedBy.Value);

                var itemNames = await _orderRepo.GetItemNamesByIdsAsync(itemIds);

				// Check that this supply coordinator has enough inventory
				var totalNeededPerItem = lineItems
					.GroupBy(l => l.ItemId)
					.ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

                var missingList = new List<string>();

				foreach (var kvp in totalNeededPerItem)
				{
					if (kvp.Value <= 0) continue;

					var available = approverInventory.ContainsKey(kvp.Key) ? approverInventory[kvp.Key] : 0;
					if (available < kvp.Value)
					{
                        var name = itemNames.ContainsKey(kvp.Key) ? itemNames[kvp.Key] : $"Mã {kvp.Key}";
                        missingList.Add($"Thi?u {name}: c?n {kvp.Value}, còn l?i {available}");
					}
				}

                if (missingList.Count > 0)
                {
                    return new StatusUpdateResultDTO 
                    { 
                        Success = false, 
                        Message = "Không ?? s? l??ng trong kho ?ê? duyê?t.", 
                        MissingItems = missingList 
                    };
                }

				// Deduct from the approving supply coordinator's inventory immediately
				order.ApprovedBy = dto.ApprovedBy.Value;
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					foreach (var line in lineItems)
					{
						if (line.Quantity <= 0) continue;
						await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, dto.ApprovedBy.Value, -line.Quantity);
						await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, dto.ApprovedBy.Value, "order_out", line.Quantity, id);
					}
				}

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Thành công" : "C?p nh?t l?i" };
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

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Thành công" : "C?p nh?t l?i" };
			}

			// When order is cancelled or rejected after approval ? restore inventory to the supply coordinator
			if ((dto.Status == "Cancelled" || dto.Status == "Rejected") && order.ApprovedBy.HasValue)
			{
				var supplierId = order.ApprovedBy.Value;
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					foreach (var line in lineItems)
					{
						if (line.Quantity <= 0) continue;
						await _orderRepo.UpdateInventoryByUserAsync(line.ItemId, supplierId, line.Quantity);
						await _orderRepo.CreateInventoryTransactionByUserAsync(line.ItemId, supplierId, "order_cancel_restore", line.Quantity, id);
					}
				}

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Thành công" : "C?p nh?t l?i" };
			}

			// For other status transitions (Processing, Cancelled from Pending, etc.)
			order.Status = dto.Status;
			var finalUpdated = await _orderRepo.UpdateOrderAsync(order);
			return new StatusUpdateResultDTO { Success = finalUpdated, Message = finalUpdated ? "Thành công" : "C?p nh?t l?i" };
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
