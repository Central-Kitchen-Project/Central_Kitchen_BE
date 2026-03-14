using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy tất cả đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(new { Status = "Success", Data = orders });
        }

        /// <summary>
        /// Lấy chi tiết 1 đơn hàng theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { Error = "OR40001", Message = "Không tìm thấy đơn hàng." });
            }
            return Ok(new { Status = "Success", Data = order });
        }

        /// <summary>
        /// Tạo đơn hàng mới (kiểm tra tồn kho trước khi tạo)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            var result = await _orderService.CreateOrderAsync(dto);

            if (!result.Success)
            {
                // Nếu thiếu hàng → trả về danh sách item bị thiếu
                if (result.InsufficientItems != null && result.InsufficientItems.Count > 0)
                {
                    return Conflict(new
                    {
                        Error = "OR40005",
                        Message = result.Message,
                        InsufficientItems = result.InsufficientItems
                    });
                }

                return BadRequest(new { Error = "OR40002", Message = result.Message });
            }

            return CreatedAtAction(nameof(GetOrderById), new { id = result.Order!.Id }, new { Status = "Success", Data = result.Order });
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto);
            if (!result.Success)
            {
                if (result.MissingItems.Count > 0)
                {
                    return BadRequest(new { 
                        Error = "OR40005", 
                        Message = result.Message,
                        MissingItems = result.MissingItems
                    });
                }
                return BadRequest(new { Error = "OR40003", Message = result.Message ?? "Cập nhật trạng thái thất bại. Trạng thái không hợp lệ hoặc không tìm thấy đơn hàng." });
            }
            return Ok(new { Status = "Success", Message = "Cập nhật trạng thái đơn hàng thành công." });
        }

        /// <summary>
        /// Xoá đơn hàng (chỉ khi trạng thái là Pending)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (!result)
            {
                return BadRequest(new { Error = "OR40004", Message = "Xoá đơn hàng thất bại. Đơn hàng không tồn tại hoặc trạng thái không phải Pending." });
            }
            return Ok(new { Status = "Success", Message = "Xoá đơn hàng thành công." });
        }
    }
}
