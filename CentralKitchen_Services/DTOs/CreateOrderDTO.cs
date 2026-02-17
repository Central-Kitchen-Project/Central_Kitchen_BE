using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class CreateOrderDTO
    {
        public int UserId { get; set; }
        public List<OrderLineItemDTO> Items { get; set; } = new List<OrderLineItemDTO>();
    }
}
