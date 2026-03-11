using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using CentralKitchen_Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IRecipeService _recipeService;
        public ItemController(IItemService itemService, IRecipeService recipeService)
        {
            _itemService = itemService;
            _recipeService = recipeService;
        }

        /// <summary>
        /// Get all items (dishes + ingredients). Filter by type, category, and/or search keyword.
        /// </summary>
        /// <param name="type">Optional: "dish" or "ingredient"</param>
        /// <param name="category">Optional: "burger", "bakery", "meat", "dairy", "sauce", "vegetable", "frozen", etc.</param>
        /// <param name="search">Optional: search by name or description</param>
        [HttpGet]
        public async Task<IActionResult> GetAllItems([FromQuery] string? type = null, [FromQuery] string? category = null, [FromQuery] string? search = null)
        {
            var items = await _itemService.GetAllItemsAsync(type, category, search);
            return Ok(new { Status = "Success", Data = items });
        }

        /// <summary>
        /// Get items filtered by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetItemsByCategory(string category)
        {
            var items = await _itemService.GetAllItemsAsync(null, category);
            return Ok(new { Status = "Success", Data = items });
        }

        /// <summary>
        /// Get a single item by ID with its ingredients (if dish)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Error = "IT40001", Message = "Item not found." });
            }
            return Ok(new { Status = "Success", Data = item });
        }

        /// <summary>
        /// Get all distinct category names (simple list for tab labels)
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _itemService.GetCategoriesAsync();
            return Ok(new { Status = "Success", Data = categories });
        }

        /// <summary>
        /// Get categories with item count (for tab badges on UI)
        /// </summary>
        [HttpGet("categories/count")]
        public async Task<IActionResult> GetCategoriesWithCount()
        {
            var categories = await _itemService.GetCategoriesWithCountAsync();
            return Ok(new { Status = "Success", Data = categories });
        }

        /// <summary>
        /// Create a new item. Inventory records (quantity 0) are automatically
        /// created for every supply coordinator and franchise store user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemDTO dto)
        {
            var item = await _itemService.CreateItemAsync(dto);
            if (item == null)
            {
                return BadRequest(new { Error = "IT40002", Message = "Failed to create item. Item name is required." });
            }
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, new { Status = "Success", Data = item });
        }

        [HttpPost("create-recipe")]
        public async Task<IActionResult> CreateFinishedProductRecipe( [FromBody] CreateFinishedProductDto dto)
        {
            if (dto == null || !dto.Ingredients.Any())
                return BadRequest("Dữ liệu nguyên liệu không được để trống.");

            try
            {
                // userId có thể dùng để kiểm tra quyền hoặc lưu log người tạo ở đây
                var success = await _itemService.CreateRecipeAsync(dto);

                if (!success)
                    return NotFound($"Không tìm thấy thành phẩm với ID {dto.FinishedItemId}");

                return Ok(new
                {
                    Message = "Đã lưu công thức thành công",
                    //CreatedBy = userId,
                    FinishedItemId = dto.FinishedItemId,
                    TotalIngredients = dto.Ingredients.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDto dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ");

            var result = await _itemService.UpdateItemAsync(id, dto);

            if (!result)
                return NotFound(new { message = $"Không tìm thấy Item với ID {id}" });

            return Ok(new { message = "Cập nhật thông tin Item thành công" });
        }
        [HttpPut("update-ingredients")]
        public async Task<IActionResult> UpdateIngredients([FromBody] UpdateRecipeDto dto)
        {
            if (dto == null || dto.FinishedItemId <= 0)
                return BadRequest("Dữ liệu thành phẩm không hợp lệ.");

            var result = await _recipeService.UpdateFinishedProductRecipeAsync(dto);

            if (result)
                return Ok(new { message = "Cập nhật danh sách nguyên liệu thành công." });

            return StatusCode(500, "Có lỗi xảy ra trong quá trình cập nhật nguyên liệu.");
        }
    }
}
