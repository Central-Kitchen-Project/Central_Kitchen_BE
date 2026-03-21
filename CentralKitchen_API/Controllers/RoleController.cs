using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Lấy tất cả roles (kèm số lượng user)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(new { Status = "Success", Data = roles });
        }

        /// <summary>
        /// Lấy role theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { Error = "RL40401", Message = "Role not found." });

            return Ok(new { Status = "Success", Data = role });
        }

        /// <summary>
        /// Tạo role mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO dto)
        {
            var role = await _roleService.CreateRoleAsync(dto);
            if (role == null)
                return BadRequest(new { Error = "RL40001", Message = "Create role failed. Role name already exists or is invalid." });

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, new { Status = "Success", Data = role });
        }

        /// <summary>
        /// Cập nhật role
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDTO dto)
        {
            var role = await _roleService.UpdateRoleAsync(id, dto);
            if (role == null)
                return BadRequest(new { Error = "RL40002", Message = "Update role failed. Role name already exists or role not found." });

            return Ok(new { Status = "Success", Data = role });
        }

        /// <summary>
        /// Xoá role (chỉ xoá được khi không còn user nào dùng role này)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
                return BadRequest(new { Error = "RL40003", Message = "Delete role failed. Role does not exist or is still in use." });

            return Ok(new { Status = "Success", Message = "Role deleted successfully." });
        }
    }
}
