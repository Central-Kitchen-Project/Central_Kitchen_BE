#nullable disable
using System;
using System.Collections.Generic;

namespace CentralKitchen_Repositories.Models;

public partial class QualityFeedback
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? OrderId { get; set; }

    public string Category { get; set; }

    public string Subject { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public DateTime? FeedbackDate { get; set; }

    public int? Rating { get; set; }  // 1-5 stars

    public virtual User User { get; set; }

    public virtual Order Order { get; set; }

}