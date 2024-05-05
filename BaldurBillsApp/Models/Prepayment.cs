using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaldurBillsApp.Models;

public partial class Prepayment
{
    public int PrepaymentId { get; set; }

    [Display(Name = "Registry number")]
    public string? PrepaymentRegistryNumber { get; set; } = null!;

    public int? VendorId { get; set; }

    [Display(Name = "Amount")]
    public decimal? PrepaymentAmount { get; set; }

    [Display(Name = "Currency")]
    public string? PrepaymentCurrency { get; set; }

    [Display(Name = "Payment date")]
    public DateOnly? PrepaymentDate { get; set; }

    [Display(Name = "Remaining amount")]
    public decimal? RemainingAmount { get; set; }

    public bool? IsSettled
    {
        get { return RemainingAmount == 0.00m; }
    }

    [Display(Name = "Entry date")]
    public DateOnly? PrepaymentEntryDate { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Vendor? Vendor { get; set; }
}
