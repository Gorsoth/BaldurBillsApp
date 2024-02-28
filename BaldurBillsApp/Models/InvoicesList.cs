using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaldurBillsApp.Models;

public partial class InvoicesList
{
    public int InvoiceId { get; set; }

    [Display(Name = "Registry number")]
    public string RegistryNumber { get; set; } = null!;

    [Display(Name = "Invoice date")]
    public DateOnly? InvoiceDate { get; set; }

    [Display(Name = "Invoice number")]
    public string? InvoiceNumber { get; set; }

    public int? VendorId { get; set; }

    public string? Title { get; set; }

    [Display(Name = "Net amount")]
    public decimal? NetAmount { get; set; }

    [Display(Name = "Gross amount")]
    public decimal? GrossAmount { get; set; }

    public string? Currency { get; set; }

    [Display(Name = "Due date")]
    public DateOnly? DueDate { get; set; }

    public bool? IsPaid { get; set; }

    [Display(Name = "Payment date")]
    public DateOnly? PaymentDate { get; set; }

    [Display(Name = "Entry date")]
    public DateOnly? EntryDate { get; set; }

    public string? Comment { get; set; }

    [Display(Name = "Rate date")]
    public DateOnly? RateDate { get; set; }

    public int? RateId { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public virtual ToPlnRate? Rate { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Vendor? Vendor { get; set; }
}
