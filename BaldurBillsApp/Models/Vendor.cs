using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class Vendor
{
    public int VendorId { get; set; }

    public string? VendorName { get; set; }

    public string? Address { get; set; }

    public string? Country { get; set; }

    public string? VatId { get; set; }

    public string? AccountNumber { get; set; }

    public string? Swift { get; set; }

    public virtual ICollection<InvoicesList> InvoicesLists { get; set; } = new List<InvoicesList>();

    public virtual ICollection<Prepayment> Prepayments { get; set; } = new List<Prepayment>();
}
