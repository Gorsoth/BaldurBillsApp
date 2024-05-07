using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using BaldurBillsApp.Services;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace BaldurBillsApp.Controllers
{
    public class InvoicesListController : Controller
    {
        private readonly BaldurBillsDbContext _context;
        private readonly ISharedDataService _sharedDataService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly PdfService _pdfService;

        public InvoicesListController(BaldurBillsDbContext context, ISharedDataService sharedDataService, IWebHostEnvironment hostingEnvironment, PdfService pdfService)
        {
            _context = context;
            _sharedDataService = sharedDataService;
            _hostingEnvironment = hostingEnvironment; // Przypisanie wstrzykniętego obiektu do lokalnej zmiennej
            _pdfService = pdfService;
        }

        // Metoda, która używa _hostingEnvironment
        public IActionResult UploadFile()
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            // logika przetwarzania pliku
            return View();
        }

        // GET: InvoicesList
        public async Task<IActionResult> Index()
        {
            var baldurBillsDbContext = _context.InvoicesLists.Include(i => i.Rate).Include(i => i.Vendor);
            return View(await baldurBillsDbContext.ToListAsync());
        }

        // GET: InvoicesList/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoicesList = await _context.InvoicesLists
                .Include(i => i.Rate)
                .Include(i => i.Vendor)
                .Include(i => i.Attachments)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoicesList == null)
            {
                return NotFound();
            }

            return View(invoicesList);
        }

        // GET: InvoicesList/Create
        public IActionResult Create(InvoicesList invoicesList = null)
        {
            ViewBag.vendors = _sharedDataService.GetVendors();
            ViewBag.CurrencyList = _sharedDataService.GetCurrencies();
            ViewBag.costTypes = _sharedDataService.GetCostTypes();
            var model = new InvoicesList();
            model.InvoiceItems = new List<InvoiceItem>(); // Zapewnia, że lista jest zainicjowana
            model = invoicesList;
            return View(model);
        }

        // POST: InvoicesList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoicesList invoicesList, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (IsInvoiceDoubled(invoicesList.VendorId, invoicesList.InvoiceNumber))
                {
                    ModelState.AddModelError("", "Invoice with this number already exists for this vendor. Please, enter different data.");
                    return Create(invoicesList);
                }

                if (invoicesList.InvoiceDate > DateOnly.FromDateTime(DateTime.Today))
                {
                    ModelState.AddModelError("", "You cannot enter an invoice with the date later than today.");
                    return Create(invoicesList);
                }
                invoicesList.EntryDate = DateOnly.FromDateTime(DateTime.Now);

                //funkcja kolejny numer w bazi / datetime miesiac / rok
                int currentMonth = DateOnly.FromDateTime(DateTime.Now).Month;
                int currentYear = DateOnly.FromDateTime(DateTime.Now).Year;
                var nextNumber = _context.InvoicesLists.Count(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == currentMonth && x.EntryDate.Value.Year == currentYear) + 1;

                var registryNumber = $@"{nextNumber}/{currentMonth}/{currentYear}";
                invoicesList.RegistryNumber = registryNumber;

                _context.Add(invoicesList);
                await _context.SaveChangesAsync();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                            if (!Directory.Exists(folderPath))
                                Directory.CreateDirectory(folderPath);

                            var filePath = Path.Combine(folderPath, Path.GetFileName(file.FileName));
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            var fileEditedPath = filePath + invoicesList.InvoiceId + ".pdf";

                            var currency = _context.ToPlnRates.FirstOrDefault(x => x.RateDate == invoicesList.RateDate && x.RateCurrency == invoicesList.Currency);
                            var plnValue = invoicesList.GrossAmount.Value * currency.RateValue;
                            var currencyEdited = currency.NbpTableName + "; 1" + currency.RateCurrency + "=" + currency.RateValue.ToString() + " Value in PLN = " + plnValue.ToString();

                            if (Path.GetExtension(filePath).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                _pdfService.AddRegistryNumberToPdf(filePath, fileEditedPath, registryNumber, currencyEdited);
                            }

                            var attachment = new Attachment
                            {
                                InvoiceId = invoicesList.InvoiceId, // Przypisz ID nowo utworzonej faktury
                                FilePath = fileEditedPath
                            };

                            _context.Attachments.Add(attachment);
                        }
                    }
                    await _context.SaveChangesAsync(); // Zapisz wszystkie załączniki
                }
            }

            return RedirectToAction(nameof(Index));

            ViewData["RateId"] = new SelectList(_context.ToPlnRates, "RateId", "RateId", invoicesList.RateId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", invoicesList.VendorId);

            return View(invoicesList);
        }

        // GET: InvoicesList/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoicesList = await _context.InvoicesLists.FindAsync(id);
            if (invoicesList == null)
            {
                return NotFound();
            }
            ViewData["RateId"] = new SelectList(_context.ToPlnRates, "RateId", "RateId", invoicesList.RateId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", invoicesList.VendorId);
            return View(invoicesList);
        }

        // POST: InvoicesList/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,RegistryNumber,InvoiceDate,InvoiceNumber,VendorId,Title,NetAmount,GrossAmount,Currency,DueDate,IsPaid,PaymentDate,EntryDate,Comment,RateDate,RateId")] InvoicesList invoicesList)
        {
            if (id != invoicesList.InvoiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoicesList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoicesListExists(invoicesList.InvoiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RateId"] = new SelectList(_context.ToPlnRates, "RateId", "RateId", invoicesList.RateId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", invoicesList.VendorId);
            return View(invoicesList);
        }

        // GET: InvoicesList/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoicesList = await _context.InvoicesLists
                .Include(i => i.Rate)
                .Include(i => i.Vendor)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoicesList == null)
            {
                return NotFound();
            }

            return View(invoicesList);
        }

        // POST: InvoicesList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoicesList = await _context.InvoicesLists.FindAsync(id);
            if (invoicesList != null)
            {
                _context.InvoicesLists.Remove(invoicesList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvoicesListExists(int id)
        {
            return _context.InvoicesLists.Any(e => e.InvoiceId == id);
        }

        private bool IsInvoiceDoubled(int? vendorId, string invoiceNumber)
        {
            bool check = _context.InvoicesLists.Any(x => x.VendorId == vendorId && x.InvoiceNumber == invoiceNumber);
            return check;
        }
    }
}
