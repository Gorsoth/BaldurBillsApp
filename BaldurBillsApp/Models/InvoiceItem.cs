using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class InvoiceItem
{
    public int ItemId { get; set; }

    public int? InvoiceId { get; set; }


    public int? VatRate { get; set; }

    public decimal? NetAmount { get; set; }

    public decimal? GrossAmount
    {
        get
        {
            if (NetAmount.HasValue && VatRate.HasValue)
            {
                // Oblicz GrossAmount na podstawie NetAmount i VatRate
                return NetAmount.Value * (1 + VatRate.Value / 100m);
            }
            else
            {
                return null;  // Jeśli jedna z wartości jest null, GrossAmount również powinien być null
            }
        }
        private set
        {
        }
    }

    public int? CostId { get; set; }

    public virtual CostType? Cost { get; set; }

    public virtual InvoicesList? Invoice { get; set; }

}
