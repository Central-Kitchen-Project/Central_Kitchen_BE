using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using CentralKitchen_Services.Services;
using CentralKitchen_Services.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CentralKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _service;
        private readonly IJwtService _jwtService;
        public AuthController(ILoginService service, IJwtService jwtService)
        {
            _service = service;
            _jwtService = jwtService;
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var result = await _service.ResetPassword(dto);
            if (!result)
            {
                return BadRequest(new { Error = "HB40005", Message = "Token invalid or reset failed." });
            }

            return Ok(new { Status = "Success", Message = "Mật khẩu đã được đặt lại thành công." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            var result = await _service.ForgotPassword(dto.Email);
            if (!result)
            {
                return BadRequest(new { Error = "HB40004", Message = "Không tìm thấy tài khoản hoặc gửi email thất bại." });
            }
            return Ok(new { Status = "Success", Message = "Yêu cầu đặt lại mật khẩu đã được gửi qua email." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO login)
        {
            var account = _service.Login(login.Email, login.Password);

            if (account == null)
            {
                return BadRequest(new { Error = "HB40001", Message = "Missing/invalid input" });
            }
            

            var token = _jwtService.GenerateToken(account);

            return Ok(new
            {
                Status = "Success",
                Username = account.Username,
                Email = account.Email,
                RoleId = account.RoleId,
               RoleName = account.Role.RoleName,
                Token = token,
                
                

            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerDto)
        {
            var isRegistered = await _service.Register(registerDto);

            if (!isRegistered)
            {
                return BadRequest(new
                {
                    Message = "Email đã tồn tại hoặc thông tin không hợp lệ."
                });
            }

            return Ok(new
            {
                Status = "Success",
                Message = "Đăng ký tài khoản thành công!"
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changeDto)
        {
            var result = await _service.ChangePassword(changeDto);

            if (!result)
            {
                return BadRequest(new
                {
                    Error = "HB40003",
                    Message = "Mật khẩu cũ không đúng hoặc không tìm thấy tài khoản."
                });
            }

            return Ok(new
            {
                Status = "Success",
                Message = "Đổi mật khẩu thành công!"
            });
        }

    }
}
