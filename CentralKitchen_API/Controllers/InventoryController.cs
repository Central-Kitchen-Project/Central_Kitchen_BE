using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        // 1. API Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllInventories();
            return Ok(result);
        }

        // 2. API Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetInventoryById(id);
            if (result == null) return NotFound("Không tìm thấy bản ghi tồn kho.");
            return Ok(result);
        }

        // 3. API Update Inventory
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] decimal quantity)
        {
            var success = await _service.UpdateStock(id, quantity);
            if (!success) return BadRequest("Cập nhật thất bại. Vui lòng kiểm tra lại ID.");

            return Ok(new { message = "Cập nhật tồn kho thành công!" });
        }
    }
}
