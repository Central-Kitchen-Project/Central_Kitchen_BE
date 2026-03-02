using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class InventoryTransactionDTO
    {
        public int Id { get; set; }
        public string TxType { get; set; } // Loại giao dịch (Nhập/Xuất...)
        public decimal? Quantity { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Thông tin chi tiết từ bảng Inventory liên kết qua
        public int InventoryId { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public string LocationName { get; set; }
    }
}
