using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Lấy tất cả users (kèm role)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { Status = "Success", Data = users });
        }

        /// <summary>
        /// Lấy user theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { Error = "US40401", Message = "Không tìm thấy user." });

            return Ok(new { Status = "Success", Data = user });
        }

        /// <summary>
        /// Lấy users theo Role ID
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            var users = await _userService.GetUsersByRoleAsync(roleId);
            return Ok(new { Status = "Success", Data = users });
        }

        /// <summary>
        /// Dashboard: đếm users theo role
        /// </summary>
        [HttpGet("dashboard/count")]
        public async Task<IActionResult> GetDashboardCount()
        {
            var count = await _userService.GetDashboardCountAsync();
            return Ok(new { Status = "Success", Data = count });
        }

        /// <summary>
        /// Cập nhật profile user (username, email)
        /// </summary>
        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDTO dto)
        {
            var user = await _userService.UpdateProfileAsync(id, dto);
            if (user == null)
                return BadRequest(new { Error = "US40001", Message = "Cập nhật thất bại. Username hoặc Email đã tồn tại." });

            return Ok(new { Status = "Success", Data = user });
        }

        /// <summary>
        /// Tạo user mới (Admin)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            if (user == null)
                return BadRequest(new { Error = "US40002", Message = "Tạo user thất bại. Username/Email đã tồn tại hoặc dữ liệu không hợp lệ." });

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new { Status = "Success", Data = user });
        }

        /// <summary>
        /// Cập nhật user (Admin - thay đổi role, password, thông tin)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            if (user == null)
                return BadRequest(new { Error = "US40003", Message = "Cập nhật user thất bại. Username/Email đã tồn tại hoặc user không tìm thấy." });

            return Ok(new { Status = "Success", Data = user });
        }

        /// <summary>
        /// Xoá user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { Error = "US40402", Message = "Không tìm thấy user để xoá." });

            return Ok(new { Status = "Success", Message = "Xoá user thành công." });
        }
    }
}
