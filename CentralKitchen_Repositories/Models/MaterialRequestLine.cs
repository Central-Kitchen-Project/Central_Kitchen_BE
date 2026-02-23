#nullable disable
using System;
using System.Collections.Generic;

namespace CentralKitchen_Repositories.Models;

public partial class MaterialRequestLine
{
    public int Id { get; set; }

    public int MaterialRequestId { get; set; }

    public int ItemId { get; set; }

    public decimal RequestedQuantity { get; set; }

    public decimal? CurrentStock { get; set; }

    public virtual MaterialRequest MaterialRequest { get; set; }

    public virtual Item Item { get; set; }
}
