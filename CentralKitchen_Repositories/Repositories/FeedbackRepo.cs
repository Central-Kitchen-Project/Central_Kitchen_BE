using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class FeedbackRepo
    {
        private readonly CentralKitchenDBContext _context;

        public FeedbackRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<List<QualityFeedback>> GetAllFeedbacksAsync()
        {
            return await _context.QualityFeedbacks
                .Include(f => f.User)
                .Include(f => f.Order)
                .OrderByDescending(f => f.FeedbackDate)
                .ToListAsync();
        }

        public async Task<QualityFeedback?> GetFeedbackByIdAsync(int id)
        {
            return await _context.QualityFeedbacks
                .Include(f => f.User)
                .Include(f => f.Order)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<QualityFeedback>> GetFeedbacksByOrderIdAsync(int orderId)
        {
            return await _context.QualityFeedbacks
                .Include(f => f.User)
                .Include(f => f.Order)
                .Where(f => f.OrderId == orderId)
                .OrderByDescending(f => f.FeedbackDate)
                .ToListAsync();
        }

        public async Task<List<QualityFeedback>> GetFeedbacksByStatusAsync(string status)
        {
            return await _context.QualityFeedbacks
                .Include(f => f.User)
                .Include(f => f.Order)
                .Where(f => f.Status == status)
                .OrderByDescending(f => f.FeedbackDate)
                .ToListAsync();
        }

        public async Task<QualityFeedback> CreateFeedbackAsync(QualityFeedback feedback)
        {
            _context.QualityFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return await GetFeedbackByIdAsync(feedback.Id);
        }

        public async Task<bool> UpdateFeedbackAsync(QualityFeedback feedback)
        {
            _context.QualityFeedbacks.Update(feedback);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            var feedback = await _context.QualityFeedbacks.FindAsync(id);
            if (feedback == null) return false;

            _context.QualityFeedbacks.Remove(feedback);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
