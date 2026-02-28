using CentralKitchen_Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IMaterialRequestService
    {
        Task<List<OrderMaterialDTO>> GetOrderMaterialsAsync(int orderId);
        Task<MaterialRequestResponseDTO?> CreateMaterialRequestAsync(CreateMaterialRequestDTO dto);
        Task<List<MaterialRequestResponseDTO>> GetAllMaterialRequestsAsync();
        Task<List<MaterialRequestResponseDTO>> GetMaterialRequestsByOrderIdAsync(int orderId);
        Task<MaterialRequestResponseDTO?> GetMaterialRequestByIdAsync(int id);
        Task<bool> UpdateMaterialRequestStatusAsync(int id, UpdateMaterialRequestStatusDTO dto);
    }
}
