using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class OrderLineItemDTO
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}
