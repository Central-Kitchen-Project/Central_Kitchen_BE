using System;
using System.Collections.Generic;

namespace CentralKitchen_Services.DTOs
{
    // POST body to submit a material request
    public class CreateMaterialRequestDTO
    {
        public int OrderId { get; set; }
        public int RequestedByUserId { get; set; }
        public string? Note { get; set; }
        public List<MaterialRequestLineDTO> Items { get; set; } = new List<MaterialRequestLineDTO>();
    }

    public class MaterialRequestLineDTO
    {
        public int ItemId { get; set; }
        public decimal RequestedQuantity { get; set; }
    }

    // Response DTO
    public class MaterialRequestResponseDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string RequestedByUsername { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<MaterialRequestLineResponseDTO> Items { get; set; } = new List<MaterialRequestLineResponseDTO>();
    }

    public class MaterialRequestLineResponseDTO
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal? CurrentStock { get; set; }
        public decimal RequestedQuantity { get; set; }
    }

    // Response for GET materials needed for an order (pre-populates the modal)
    public class OrderMaterialDTO
    {
        public int ItemId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityNeeded { get; set; }
        public decimal CurrentStock { get; set; }
    }

    // Update status DTO
    public class UpdateMaterialRequestStatusDTO
    {
        public string Status { get; set; }
    }
}
