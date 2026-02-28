using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class InventoryDTO
    {
        public int Id { get; set; }
        public decimal? Quantity { get; set; }

        // Thông tin chi tiết của Item
        public ItemDTO Item { get; set; }

        // Thông tin chi tiết của Location
        public LocationDTO Location { get; set; }
    }

    public class ItemDTO
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string ItemType { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
    }

    public class LocationDTO
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
    }
}
