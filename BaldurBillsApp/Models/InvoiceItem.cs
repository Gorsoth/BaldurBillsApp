using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class InvoiceItem
{
    public int ItemId { get; set; }

    public int? InvoiceId { get; set; }

    public int? VatRate { get; set; }

    public decimal? NetAmount { get; set; }

    public decimal? GrossAmount { get; set; }

    public int? CostId { get; set; }

    public virtual CostType? Cost { get; set; }

    public virtual InvoicesList? Invoice { get; set; }
}
