using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class RecipeIngredientDto
    {
        public int IngredientItemId { get; set; } // ID của nguyên liệu (trong bảng items)
        public decimal Quantity { get; set; }     // Số lượng nguyên liệu cần dùng
    }
}
