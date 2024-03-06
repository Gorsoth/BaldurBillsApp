using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaldurBillsApp.Models;

public partial class Vendor
{
    [Required]
    [Display(Name = "ID")]
    public int VendorId { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string? VendorName { get; set; }

    public string? Address { get; set; }

    public string? Country { get; set; }

    [Display(Name = "VAT ID")]
    public string? VatId { get; set; }

    [Display(Name = "Account number")]
    public string? AccountNumber { get; set; }

    [Display(Name = "SWIFT")]
    public string? Swift { get; set; }

    public virtual ICollection<InvoicesList> InvoicesLists { get; set; } = new List<InvoicesList>();

    public virtual ICollection<Prepayment> Prepayments { get; set; } = new List<Prepayment>();
}
