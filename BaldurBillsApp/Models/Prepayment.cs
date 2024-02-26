using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class Prepayment
{
    public int PrepaymentId { get; set; }

    public string PrepaymentRegistryNumber { get; set; } = null!;

    public int? VendorId { get; set; }

    public decimal? PrepaymentAmount { get; set; }

    public string? PrepaymentCurrency { get; set; }

    public DateOnly? PrepaymentDate { get; set; }

    public decimal? RemainingAmount { get; set; }

    public bool? IsSettled { get; set; }

    public DateOnly? PrepaymentEntryDate { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Vendor? Vendor { get; set; }
}
