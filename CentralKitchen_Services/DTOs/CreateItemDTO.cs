namespace CentralKitchen_Services.DTOs
{
    public class CreateItemDTO
    {
        public string ItemName { get; set; }
        public string? Unit { get; set; }
        public string? ItemType { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
    }
}
