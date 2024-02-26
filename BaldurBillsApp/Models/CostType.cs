using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class CostType
{
    public int CostId { get; set; }

    public string? CostName { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
