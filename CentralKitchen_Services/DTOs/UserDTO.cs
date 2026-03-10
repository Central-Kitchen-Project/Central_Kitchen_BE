using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateProfileDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public string? Password { get; set; }
    }

    public class DashboardCountDTO
    {
        public int TotalUsers { get; set; }
        public List<RoleCountDTO> RoleCounts { get; set; } = new List<RoleCountDTO>();
    }

    public class RoleCountDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int Count { get; set; }
    }
}
