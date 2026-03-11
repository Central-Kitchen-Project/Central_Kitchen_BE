using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class ItemUpdateDto
    {
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string ItemType { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }
}
