using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaldurBillsApp.Models;

public partial class ToPlnRate
{
    public int RateId { get; set; }

    [Display(Name = "Rate date")]
    public DateOnly? RateDate { get; set; }

    [Display(Name = "Currency")]
    public string? RateCurrency { get; set; }

    [Display(Name = "Value")]
    public decimal? RateValue { get; set; }

    public virtual ICollection<InvoicesList> InvoicesLists { get; set; } = new List<InvoicesList>();
}
