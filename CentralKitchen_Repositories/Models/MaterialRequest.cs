#nullable disable
using System;
using System.Collections.Generic;

namespace CentralKitchen_Repositories.Models;

public partial class MaterialRequest
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int RequestedByUserId { get; set; }

    public string Status { get; set; }

    public string Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual User RequestedByUser { get; set; }

    public virtual ICollection<MaterialRequestLine> MaterialRequestLines { get; set; } = new List<MaterialRequestLine>();
}
