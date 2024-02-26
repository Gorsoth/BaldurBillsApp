using System;
using System.Collections.Generic;

namespace BaldurBillsApp.Models;

public partial class Attachment
{
    public int AttachmentId { get; set; }

    public int? InvoiceId { get; set; }

    public string? FilePath { get; set; }

    public virtual InvoicesList? Invoice { get; set; }
}
