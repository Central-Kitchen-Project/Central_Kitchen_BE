using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IFeedbackService
    {
        Task<List<FeedbackResponseDTO>> GetAllFeedbacksAsync();
        Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id);
        Task<List<FeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(int orderId);
        Task<List<FeedbackResponseDTO>> GetFeedbacksByStatusAsync(string status);
        Task<FeedbackResponseDTO?> CreateFeedbackAsync(CreateFeedbackDTO dto);
        Task<bool> UpdateFeedbackStatusAsync(int id, UpdateFeedbackStatusDTO dto);
        Task<bool> DeleteFeedbackAsync(int id);
    }
}
