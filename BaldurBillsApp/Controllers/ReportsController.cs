using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Hosting;
using BaldurBillsApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authorization;

namespace BaldurBillsApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly BaldurBillsDbContext _context;
        private readonly PdfService _pdfService;

        public ReportsController(BaldurBillsDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View();
        }

        // GET: Reports/Payments
        public async Task<IActionResult> Payments(DateOnly? startDate, DateOnly? endDate)
        {
            var payments = _context.Settlements.Include(s => s.Invoice).ThenInclude(i => i.Vendor).AsQueryable();

            if (startDate.HasValue)
            {
                var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
                payments = payments.Where(p => p.SettlementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
                payments = payments.Where(p => p.SettlementDate <= endDate.Value);
            }

            ViewData["startDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["endDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(await payments.ToListAsync());
        }

        // GET: Reports/PaymentsPdf
        public async Task<IActionResult> PaymentsPdf(DateOnly? startDate, DateOnly? endDate)
        {
            var payments = _context.Settlements.Include(s => s.Invoice).ThenInclude(i => i.Vendor).AsQueryable();

            if (startDate.HasValue)
            {
                var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
                payments = payments.Where(p => p.SettlementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
                payments = payments.Where(p => p.SettlementDate <= endDate.Value);
            }

            var paymentsList = await payments.ToListAsync();
            var textContent = GeneratePaymentsReportText(paymentsList);

            var pdf = _pdfService.GeneratePdf(textContent);
            return File(pdf, "application/pdf", "PaymentsReport.pdf");
        }

        // GET: Reports/UnpaidInvoices
        public async Task<IActionResult> UnpaidInvoices(int? vendorId)
        {
            var unpaidInvoices = _context.InvoicesLists.Include(i => i.Vendor)
                                                       .Where(i => i.IsPaid == false || i.IsPaid == null)
                                                       .AsQueryable();

            if (vendorId.HasValue)
            {
                unpaidInvoices = unpaidInvoices.Where(i => i.VendorId == vendorId);
            }

            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.SelectedVendor = vendorId;
            return View(await unpaidInvoices.ToListAsync());
        }

        // GET: Reports/UnpaidInvoicesPdf
        public async Task<IActionResult> UnpaidInvoicesPdf(int? vendorId)
        {
            var unpaidInvoices = _context.InvoicesLists.Include(i => i.Vendor)
                                                       .Where(i => i.IsPaid == false || i.IsPaid == null)
                                                       .AsQueryable();

            if (vendorId.HasValue)
            {
                unpaidInvoices = unpaidInvoices.Where(i => i.VendorId == vendorId);
            }

            var unpaidInvoicesList = await unpaidInvoices.ToListAsync();
            var textContent = GenerateUnpaidInvoicesReportText(unpaidInvoicesList);

            var pdf = _pdfService.GeneratePdf(textContent);
            return File(pdf, "application/pdf", "UnpaidInvoicesReport.pdf");
        }

        private string GenerateUnpaidInvoicesReportText(IEnumerable<InvoicesList> invoices)
        {
            var report = new StringWriter();
            report.WriteLine("Unpaid Invoices Report");
            report.WriteLine("==================================");

            foreach (var invoice in invoices)
            {
                report.WriteLine($"Invoice Number: {invoice.InvoiceNumber}");
                report.WriteLine($"Vendor: {invoice.Vendor.VendorName}");
                report.WriteLine($"Title: {invoice.Title}");
                report.WriteLine($"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}");
                report.WriteLine($"Due Date: {(invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("yyyy-MM-dd") : "N/A")}");
                report.WriteLine($"Gross Amount: {invoice.GrossAmount}");
                report.WriteLine("----------------------------------");
            }

            return report.ToString();
        }

        private string GeneratePaymentsReportText(IEnumerable<Settlement> settlements)
        {
            var report = new StringWriter();
            report.WriteLine("Payments Report");
            report.WriteLine("==================================");

            foreach (var settlement in settlements)
            {
                report.WriteLine($"Invoice Number: {settlement.Invoice.InvoiceNumber}");
                report.WriteLine($"Vendor: {settlement.Invoice.Vendor.VendorName}");
                report.WriteLine($"Settlement Date: {settlement.SettlementDate:yyyy-MM-dd}");
                report.WriteLine($"Settlement Amount: {settlement.SettlementAmount}");
                report.WriteLine("----------------------------------");
            }

            return report.ToString();
        }
    }
}