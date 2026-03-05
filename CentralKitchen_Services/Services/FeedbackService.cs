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
    public class FeedbackService : IFeedbackService
    {
        private readonly FeedbackRepo _feedbackRepo;

        private static readonly List<string> ValidCategories = new() { "Quality", "Packaging", "Delivery" };
        private static readonly List<string> ValidStatuses = new() { "Received", "Under Review", "Resolved" };

        public FeedbackService(FeedbackRepo feedbackRepo)
        {
            _feedbackRepo = feedbackRepo;
        }

        public async Task<List<FeedbackResponseDTO>> GetAllFeedbacksAsync()
        {
            var feedbacks = await _feedbackRepo.GetAllFeedbacksAsync();
            return feedbacks.Select(MapToResponseDTO).ToList();
        }

        public async Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id)
        {
            var feedback = await _feedbackRepo.GetFeedbackByIdAsync(id);
            return feedback != null ? MapToResponseDTO(feedback) : null;
        }

        public async Task<List<FeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(int orderId)
        {
            var feedbacks = await _feedbackRepo.GetFeedbacksByOrderIdAsync(orderId);
            return feedbacks.Select(MapToResponseDTO).ToList();
        }

        public async Task<List<FeedbackResponseDTO>> GetFeedbacksByStatusAsync(string status)
        {
            var feedbacks = await _feedbackRepo.GetFeedbacksByStatusAsync(status);
            return feedbacks.Select(MapToResponseDTO).ToList();
        }

        public async Task<FeedbackResponseDTO?> CreateFeedbackAsync(CreateFeedbackDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Subject))
                return null;

            if (string.IsNullOrWhiteSpace(dto.Category) || !ValidCategories.Contains(dto.Category))
                return null;

            var feedback = new QualityFeedback
            {
                UserId = dto.UserId,
                OrderId = dto.OrderId,
                Category = dto.Category,
                Subject = dto.Subject,
                Description = dto.Description,
                Status = "Received",
                FeedbackDate = DateTime.Now
            };

            var created = await _feedbackRepo.CreateFeedbackAsync(feedback);
            return created != null ? MapToResponseDTO(created) : null;
        }

        public async Task<bool> UpdateFeedbackStatusAsync(int id, UpdateFeedbackStatusDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status) || !ValidStatuses.Contains(dto.Status))
                return false;

            var feedback = await _feedbackRepo.GetFeedbackByIdAsync(id);
            if (feedback == null) return false;

            feedback.Status = dto.Status;
            return await _feedbackRepo.UpdateFeedbackAsync(feedback);
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            return await _feedbackRepo.DeleteFeedbackAsync(id);
        }

        // ===== Helper: Map Entity → DTO =====
        private FeedbackResponseDTO MapToResponseDTO(QualityFeedback feedback)
        {
            return new FeedbackResponseDTO
            {
                Id = feedback.Id,
                UserId = feedback.UserId,
                Username = feedback.User?.Username ?? "",
                OrderId = feedback.OrderId,
                RefId = feedback.OrderId.HasValue ? $"#ORD-{feedback.OrderId.Value}" : null,
                Category = feedback.Category,
                Subject = feedback.Subject,
                Description = feedback.Description,
                Status = feedback.Status,
                FeedbackDate = feedback.FeedbackDate
            };
        }
    }
}
