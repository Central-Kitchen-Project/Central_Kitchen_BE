using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class CreateOrderResultDTO
    {
        public bool Success { get; set; }
        public OrderResponseDTO? Order { get; set; }
        public string? Message { get; set; }
        public List<InsufficientItemDTO>? InsufficientItems { get; set; }
    }

    public class InsufficientItemDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal ShortageQuantity { get; set; }
    }
}
