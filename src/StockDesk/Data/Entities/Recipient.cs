using System;
using System.Collections.Generic;

namespace StockDesk.Data.Entities;

public class Recipient
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<InventoryOperation> Operations { get; set; } = new List<InventoryOperation>();
}
