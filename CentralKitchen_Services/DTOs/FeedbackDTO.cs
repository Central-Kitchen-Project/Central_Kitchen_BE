using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class CreateFeedbackDTO
    {
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public string Category { get; set; }       // Quality, Packaging, Delivery
        public string Subject { get; set; }
        public string Description { get; set; }
    }

    public class UpdateFeedbackStatusDTO
    {
        public string Status { get; set; }          // Received, Under Review, Resolved
    }

    public class FeedbackResponseDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public int? OrderId { get; set; }
        public string RefId { get; set; }           // #ORD-XXXX
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? FeedbackDate { get; set; }
    }
}
