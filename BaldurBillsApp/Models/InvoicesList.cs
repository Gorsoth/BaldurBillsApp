using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class InvoicesList
{
    public int InvoiceId { get; set; }

    public string RegistryNumber { get; set; } = null!;

    public DateOnly? InvoiceDate { get; set; }

    public string? InvoiceNumber { get; set; }

    public int? VendorId { get; set; }

    public string? Title { get; set; }

    public decimal? NetAmount { get; set; }

    public decimal? GrossAmount { get; set; }

    public string? Currency { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? IsPaid { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public DateOnly? EntryDate { get; set; }

    public string? Comment { get; set; }

    public DateOnly? RateDate { get; set; }

    public int? RateId { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public virtual ToPlnRate? Rate { get; set; }

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual Vendor? Vendor { get; set; }
}
