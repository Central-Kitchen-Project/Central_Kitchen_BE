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

		// Danh s�ch tr?ng th�i h?p l?
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
				return new CreateOrderResultDTO { Success = false, Message = "Product list cannot be empty." };

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
				Message = "Order created successfully.",
				Order = createdOrder != null ? MapToResponseDTO(createdOrder) : null
			};
		}

		public async Task<StatusUpdateResultDTO> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO dto)
		{
			if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
				return new StatusUpdateResultDTO { Success = false, Message = "Invalid status." };

			var order = await _orderRepo.GetOrderByIdAsync(id);
			if (order == null) return new StatusUpdateResultDTO { Success = false, Message = "Order not found." };

			// Tránh xử lý lại đơn hàng
			if (order.Status == dto.Status) return new StatusUpdateResultDTO { Success = false, Message = "Order is already in this status." };

			// Luu tru du lieu chi tiet don hang truoc khi cap nhat trang thai de phuc vu qua trinh tru/cong kho
			var orderUserId = order.UserId;

			var rawLineItems = new List<(int ItemId, decimal Quantity)>();
			foreach (var ol in order.OrderLines)
			{
				var qty = ol.Quantity ?? 0m;
				if (qty <= 0) continue;

				if (ol.Item != null && ol.Item.RecipeFinishedItems != null && ol.Item.RecipeFinishedItems.Any())
				{
					// Day la thanh pham, can phan ra thanh cac nguyen lieu con
					foreach (var recipe in ol.Item.RecipeFinishedItems)
					{
						rawLineItems.Add((recipe.IngredientItemId, recipe.Quantity * qty));
					}
				}
				else
				{
					// Day la nguyen lieu tho hoac mon khong co cong thuc
					rawLineItems.Add((ol.ItemId, qty));
				}
			}

			// Gop chung so luong cua cac mon trung lap
			var lineItems = rawLineItems
				.GroupBy(x => x.ItemId)
				.Select(g => (ItemId: g.Key, Quantity: g.Sum(x => x.Quantity)))
				.ToList();

			// Khi xac nhan don (Approved), tien hanh kiem tra va tru kho cua Supply Coordinator truc tiep duyet don
			if (dto.Status == "Approved" || dto.Status == "Delivery")
			{
				if (!dto.ApprovedBy.HasValue || dto.ApprovedBy.Value <= 0)
					return new StatusUpdateResultDTO { Success = false, Message = "Invalid approver." };

				var approverRole = await _orderRepo.GetUserRoleNameAsync(dto.ApprovedBy.Value);
				if (approverRole != "SupplyCoordinator")
					return new StatusUpdateResultDTO { Success = false, Message = "Only Supply Coordinator can approve orders." };

				var itemIds = lineItems.Select(l => l.ItemId).Distinct().ToList();
				var approverInventory = await _orderRepo.GetInventoryByUserAndItemsAsync(itemIds, dto.ApprovedBy.Value);

                var itemNames = await _orderRepo.GetItemNamesByIdsAsync(itemIds);

				// Kiem tra xem Supply Coordinator nay co du ton kho nguyen lieu hay khong
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
                        var name = itemNames.ContainsKey(kvp.Key) ? itemNames[kvp.Key] : $"Code {kvp.Key}";
                        missingList.Add($"Missing {name}: need {kvp.Value}, remaining {available}");
					}
				}

                if (missingList.Count > 0)
                {
                    return new StatusUpdateResultDTO 
                    { 
                        Success = false, 
                        Message = "Not enough inventory to approve.", 
                        MissingItems = missingList 
                    };
                }

				// Lap tuc tru so luong xuat khoi kho cua Supply Coordinator duyet don
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

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Success" : "Update failed" };
			}

			// Khi don hang hoan tat -> cong kho cho chi nhanh cua hang nhuong quyen
			if (dto.Status == "Completed")
			{
				order.Status = dto.Status;
				var updated = await _orderRepo.UpdateOrderAsync(order);

				if (updated)
				{
					await AddInventoryToFranchise(orderUserId, lineItems, id);
				}

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Success" : "Update failed" };
			}

			// Khi don hang bi huy hoac tu choi sau khi da duyet -> hoan tra lai ton kho cho Supply Coordinator
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

				return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Success" : "Update failed" };
			}

			// Doi voi cac chuyen doi trang thai khac (Processing, Cancelled tu Pending, v.v.)
			order.Status = dto.Status;
			var finalUpdated = await _orderRepo.UpdateOrderAsync(order);
			return new StatusUpdateResultDTO { Success = finalUpdated, Message = finalUpdated ? "Success" : "Update failed" };
		}

		/// <summary>
		/// Cộng kho cho cửa hàng nhượng quyền khi đơn hàng hoàn tất.
		/// (Tồn kho của Supply Coordinator đã được trừ sẵn lúc duyệt đơn).
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

			// Ch? cho ph�p xo� ??n h�ng c� tr?ng th�i "Pending"
			if (order.Status != "Pending")
				return false;

			return await _orderRepo.DeleteOrderAsync(id);
		}

		// ===== Helper: Map Entity -> DTO =====
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
