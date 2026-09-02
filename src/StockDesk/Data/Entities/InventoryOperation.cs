using System;

namespace StockDesk.Data.Entities;

public enum OperationType
{
    Inflow = 1,  // Mədaxil (Artırılma / Daxil olma)
    Outflow = 2  // Məxaric (Təhvil verilmə / Silinmə)
}

public class InventoryOperation
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public OperationType OperationType { get; set; }

    public int? ProductId { get; set; }

    public virtual Product? Product { get; set; }

    public string ProductNameSnapshot { get; set; } = string.Empty;

    public string CategoryNameSnapshot { get; set; } = string.Empty;

    public int QuantityDelta { get; set; }

    public int? RecipientId { get; set; }

    public virtual Recipient? Recipient { get; set; }

    public string? RecipientNameSnapshot { get; set; }

    public string? Note { get; set; }
}
