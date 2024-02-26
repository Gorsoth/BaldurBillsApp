using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class Settlement
{
    public int SettlementId { get; set; }

    public int? InvoiceId { get; set; }

    public DateOnly? SettlementDate { get; set; }

    public decimal? SettlementAmount { get; set; }

    public int? PrepaymentId { get; set; }

    public virtual InvoicesList? Invoice { get; set; }

    public virtual Prepayment? Prepayment { get; set; }
}
