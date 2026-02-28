using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Lấy tất cả feedback (Past Feedback History)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return Ok(new { Status = "Success", Data = feedbacks });
        }

        /// <summary>
        /// Lấy feedback theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return NotFound(new { Error = "FB40401", Message = "Không tìm thấy feedback." });

            return Ok(new { Status = "Success", Data = feedback });
        }

        /// <summary>
        /// Lấy feedback theo Item ID
        /// </summary>
        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetFeedbacksByItemId(int itemId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByItemIdAsync(itemId);
            return Ok(new { Status = "Success", Data = feedbacks });
        }

        /// <summary>
        /// Lọc feedback theo Status (Filter: All Status)
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetFeedbacksByStatus(string status)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByStatusAsync(status);
            return Ok(new { Status = "Success", Data = feedbacks });
        }

        /// <summary>
        /// Submit New Feedback (Submit Report)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDTO dto)
        {
            var feedback = await _feedbackService.CreateFeedbackAsync(dto);
            if (feedback == null)
                return BadRequest(new
                {
                    Error = "FB40001",
                    Message = "Tạo feedback thất bại. Kiểm tra lại Category (Quality/Packaging/Delivery) và Subject không được để trống."
                });

            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, new { Status = "Success", Data = feedback });
        }

        /// <summary>
        /// Cập nhật trạng thái feedback (Received → Under Review → Resolved)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, [FromBody] UpdateFeedbackStatusDTO dto)
        {
            var result = await _feedbackService.UpdateFeedbackStatusAsync(id, dto);
            if (!result)
                return BadRequest(new
                {
                    Error = "FB40002",
                    Message = "Cập nhật trạng thái thất bại. Kiểm tra lại Status (Received/Under Review/Resolved)."
                });

            return Ok(new { Status = "Success", Message = "Cập nhật trạng thái feedback thành công." });
        }

        /// <summary>
        /// Xoá feedback
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (!result)
                return NotFound(new { Error = "FB40402", Message = "Không tìm thấy feedback để xoá." });

            return Ok(new { Status = "Success", Message = "Xoá feedback thành công." });
        }
    }
}
