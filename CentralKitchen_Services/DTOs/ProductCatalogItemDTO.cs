using System.Collections.Generic;

namespace CentralKitchen_Services.DTOs
{
    public class ProductCatalogItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public List<IngredientDTO> Ingredients { get; set; } = new List<IngredientDTO>();
    }
}
