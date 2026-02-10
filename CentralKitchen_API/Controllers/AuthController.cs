using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
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
    }
}
