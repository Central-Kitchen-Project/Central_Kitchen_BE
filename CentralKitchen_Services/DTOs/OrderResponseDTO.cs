using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public List<OrderLineResponseDTO> OrderLines { get; set; } = new List<OrderLineResponseDTO>();
    }

    public class OrderLineResponseDTO
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
    }
}
