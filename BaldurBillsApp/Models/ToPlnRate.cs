using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class ToPlnRate
{
    public int RateId { get; set; }

    public DateOnly? RateDate { get; set; }

    public string? RateCurrency { get; set; }

    public decimal? RateValue { get; set; }

    public virtual ICollection<InvoicesList> InvoicesLists { get; set; } = new List<InvoicesList>();
}
