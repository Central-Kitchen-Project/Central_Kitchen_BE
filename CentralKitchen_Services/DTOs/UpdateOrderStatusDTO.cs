using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class UpdateOrderStatusDTO
    {
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
    }
}
