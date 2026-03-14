using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialRequestController : ControllerBase
    {
        private readonly IMaterialRequestService _service;

        public MaterialRequestController(IMaterialRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get materials/ingredients needed for an order (pre-populates the Request modal)
        /// Shows material name, current stock, and quantity needed
        /// </summary>
        [HttpGet("order/{orderId}/materials")]
        public async Task<IActionResult> GetOrderMaterials(int orderId)
        {
            var materials = await _service.GetOrderMaterialsAsync(orderId);
            return Ok(new { Status = "Success", Data = materials });
        }

        /// <summary>
        /// Submit a material request for an order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMaterialRequest([FromBody] CreateMaterialRequestDTO dto)
        {
            var result = await _service.CreateMaterialRequestAsync(dto);
            if (result == null)
            {
                return BadRequest(new { Error = "MR40001", Message = "Danh sách v?t t? không ???c ?? tr?ng." });
            }

            return CreatedAtAction(nameof(GetMaterialRequestById), new { id = result.Id }, new { Status = "Success", Data = result });
        }

        /// <summary>
        /// Get all material requests
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMaterialRequests()
        {
            var requests = await _service.GetAllMaterialRequestsAsync();
            return Ok(new { Status = "Success", Data = requests });
        }

        /// <summary>
        /// Get material requests for a specific order
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetMaterialRequestsByOrderId(int orderId)
        {
            var requests = await _service.GetMaterialRequestsByOrderIdAsync(orderId);
            return Ok(new { Status = "Success", Data = requests });
        }

        /// <summary>
        /// Get a specific material request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterialRequestById(int id)
        {
            var request = await _service.GetMaterialRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { Error = "MR40002", Message = "Không tìm th?y yêu c?u v?t t?." });
            }
            return Ok(new { Status = "Success", Data = request });
        }

        /// <summary>
        /// Update material request status (Pending ? Approved/Rejected/Fulfilled)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMaterialRequestStatusDTO dto)
        {
            var result = await _service.UpdateMaterialRequestStatusAsync(id, dto);
            if (!result.Success)
            {
                if (result.MissingItems != null && result.MissingItems.Count > 0)
                {
                    return BadRequest(new { 
                        Error = "MR40004", 
                        Message = result.Message,
                        MissingItems = result.MissingItems
                    });
                }
                return BadRequest(new { Error = "MR40003", Message = result.Message ?? "C?p nh?t tr?ng thái th?t b?i." });
            }
            return Ok(new { Status = "Success", Message = "C?p nh?t tr?ng thái yêu c?u v?t t? thành công." });
        }
    }
}
