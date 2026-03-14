using System.Collections.Generic;

namespace CentralKitchen_Services.DTOs
{
    public class StatusUpdateResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> MissingItems { get; set; } = new List<string>();
    }
}