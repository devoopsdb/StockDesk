using System;
using System.Collections.Generic;

namespace StockDesk.Data.Entities;

public class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? ImageFileName { get; set; }

    public int CurrentBalance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<InventoryOperation> Operations { get; set; } = new List<InventoryOperation>();
}
