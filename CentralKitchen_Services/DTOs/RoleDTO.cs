using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class RoleResponseDTO
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
    }

    public class CreateRoleDTO
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
    }

    public class UpdateRoleDTO
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}
