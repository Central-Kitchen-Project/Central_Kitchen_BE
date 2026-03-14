using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class MaterialRequestService : IMaterialRequestService
    {
        private readonly MaterialRequestRepo _repo;

        private static readonly List<string> ValidStatuses = new List<string>
        {
            "Pending", "Approved", "Processing", "Rejected", "Fulfilled"
        };

        public MaterialRequestService(MaterialRequestRepo repo)
        {
            _repo = repo;
        }

        public async Task<List<OrderMaterialDTO>> GetOrderMaterialsAsync(int orderId)
        {
            var materials = await _repo.GetOrderMaterialsAsync(orderId);
            return materials.Select(m => new OrderMaterialDTO
            {
                ItemId = m.ItemId,
                MaterialName = m.ItemName,
                Unit = m.Unit,
                QuantityNeeded = m.QuantityNeeded,
                CurrentStock = m.CurrentStock
            }).ToList();
        }

        public async Task<MaterialRequestResponseDTO?> CreateMaterialRequestAsync(CreateMaterialRequestDTO dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return null;

            // Get current stock for each item to store snapshot
            var itemIds = dto.Items.Select(i => i.ItemId).ToList();
            var materials = await _repo.GetOrderMaterialsAsync(dto.OrderId);
            var stockMap = materials.ToDictionary(m => m.ItemId, m => m.CurrentStock);

            var request = new MaterialRequest
            {
                OrderId = dto.OrderId,
                RequestedByUserId = dto.RequestedByUserId,
                Status = "Pending",
                Note = dto.Note,
                CreatedAt = DateTime.Now,
                MaterialRequestLines = dto.Items.Select(item => new MaterialRequestLine
                {
                    ItemId = item.ItemId,
                    RequestedQuantity = item.RequestedQuantity,
                    CurrentStock = stockMap.ContainsKey(item.ItemId) ? stockMap[item.ItemId] : 0
                }).ToList()
            };

            var created = await _repo.CreateAsync(request);
            if (created == null) return null;

            return MapToResponseDTO(created);
        }

        public async Task<List<MaterialRequestResponseDTO>> GetAllMaterialRequestsAsync()
        {
            var requests = await _repo.GetAllAsync();
            return requests.Select(MapToResponseDTO).ToList();
        }

        public async Task<List<MaterialRequestResponseDTO>> GetMaterialRequestsByOrderIdAsync(int orderId)
        {
            var requests = await _repo.GetByOrderIdAsync(orderId);
            return requests.Select(MapToResponseDTO).ToList();
        }

        public async Task<MaterialRequestResponseDTO?> GetMaterialRequestByIdAsync(int id)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null) return null;
            return MapToResponseDTO(request);
        }

        public async Task<StatusUpdateResultDTO> UpdateMaterialRequestStatusAsync(int id, UpdateMaterialRequestStatusDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Status) || !ValidStatuses.Contains(dto.Status))
                return new StatusUpdateResultDTO { Success = false, Message = "Tr?ng thái không h?p l?." };

            if (dto.Status == "Approved" || dto.Status == "Fulfilled")
            {
                var repoResult = await _repo.ApproveAndUpdateInventoryAsync(id, dto.Status);
                return new StatusUpdateResultDTO 
                { 
                    Success = repoResult.Success, 
                    Message = repoResult.Message, 
                    MissingItems = repoResult.MissingList 
                };
            }

            var updated = await _repo.UpdateStatusAsync(id, dto.Status);
            return new StatusUpdateResultDTO { Success = updated, Message = updated ? "Thành công" : "C?p nh?t l?i" };
        }

        private MaterialRequestResponseDTO MapToResponseDTO(MaterialRequest mr)
        {
            return new MaterialRequestResponseDTO
            {
                Id = mr.Id,
                OrderId = mr.OrderId,
                RequestedByUsername = mr.RequestedByUser?.Username ?? "",
                Status = mr.Status ?? "",
                Note = mr.Note,
                CreatedAt = mr.CreatedAt,
                Items = mr.MaterialRequestLines.Select(l => new MaterialRequestLineResponseDTO
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    MaterialName = l.Item?.ItemName ?? "",
                    Unit = l.Item?.Unit ?? "",
                    CurrentStock = l.CurrentStock,
                    RequestedQuantity = l.RequestedQuantity
                }).ToList()
            };
        }
    }
}
