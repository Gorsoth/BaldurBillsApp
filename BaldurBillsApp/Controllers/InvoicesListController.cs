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
using BaldurBillsApp.ViewModels;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AutoMapper;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using Microsoft.AspNetCore.Authorization;

namespace BaldurBillsApp.Controllers
{
    [Authorize]
    public class InvoicesListController : Controller
    {
        private readonly BaldurBillsDbContext _context;
        private readonly ISharedDataService _sharedDataService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly PdfService _pdfService;
        private readonly IMapper _mapper;

        public InvoicesListController(BaldurBillsDbContext context, ISharedDataService sharedDataService, IWebHostEnvironment hostingEnvironment, PdfService pdfService, IMapper mapper)
        {
            _context = context;
            _sharedDataService = sharedDataService;
            _hostingEnvironment = hostingEnvironment; // Przypisanie wstrzykniętego obiektu do lokalnej zmiennej
            _pdfService = pdfService;
            _mapper = mapper;
        }

        // Metoda, która używa _hostingEnvironment
        public IActionResult UploadFile()
        {
            var path = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            // logika przetwarzania pliku
            return View();
        }

        // GET: InvoicesList
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var invoices = _context.InvoicesLists
                .Include(i => i.Rate)
                .Include(i => i.Vendor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                invoices = invoices.Where(i =>
                    i.RegistryNumber.Contains(searchTerm) ||
                    i.InvoiceNumber.Contains(searchTerm) ||
                    i.Vendor.VendorName.Contains(searchTerm) ||
                    i.Title.Contains(searchTerm) ||
                    i.InvoiceDate.ToString().Contains(searchTerm) ||
                    i.GrossAmount.ToString().Contains(searchTerm) ||
                    i.NetAmount.ToString().Contains(searchTerm) ||
                    i.Currency.Contains(searchTerm));
            }

            return View(await invoices.ToListAsync());
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

        public decimal CalculateRemainingAmount(decimal? invoiceAmount, List<Settlement> settlementAmount)
        {
            decimal totalSettlements = settlementAmount.Sum(s => s.SettlementAmount) ?? 0;
            decimal remainingAmount = (invoiceAmount ?? 0) - totalSettlements;
            return remainingAmount;
        }

        // GET: InvoicesList/Settlement/{invoiceId}
        [HttpGet]
        public IActionResult Settlement(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoicesList = _context.InvoicesLists.FirstOrDefault(i => i.InvoiceId == id);
            if (invoicesList == null)
            {
                return NotFound();
            }

            var settlements = _context.Settlements.Where(s => s.InvoiceId == id).ToList();

            decimal remainingAmount = CalculateRemainingAmount(invoicesList.GrossAmount, settlements);

            var prepayments = _context.Prepayments
                .Where(p => p.VendorId == invoicesList.VendorId)
                .AsEnumerable() 
                .Where(p => (p.IsSettled == false || p.IsSettled == null) && p.RemainingAmount > 0)
                .ToList(); 
            var prepaymentsViewModel = _mapper.Map<List<PrepaymentViewModel>>(prepayments);

            var viewModel = new SettlementViewModel
            {
                InvoiceList = invoicesList,
                Settlements = settlements,
                RemainingAmount = remainingAmount,
                Prepayments = prepaymentsViewModel
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessPrepayments(int invoiceId, int[] selectedPrepayments)
        {
            var invoice = _context.InvoicesLists.FirstOrDefault(i => i.InvoiceId == invoiceId);
            var settlements = _context.Settlements.Where(s => s.InvoiceId == invoiceId).ToList();

            decimal? remainingAmount = CalculateRemainingAmount(invoice.GrossAmount, settlements);

            if (invoice == null)
            {
                return NotFound();
            }

            var counter = 0;
            // Logika przetwarzania przedpłat
            for (int i = 0; i < selectedPrepayments.Length; i++)
            {
                counter = i + 1;
                var prepayment = _context.Prepayments.FirstOrDefault( p=> p.PrepaymentId == selectedPrepayments[i]);
                if (prepayment != null)
                {
                    remainingAmount -= prepayment.RemainingAmount;
                    if (remainingAmount <= 0)
                    {
                        break;
                    }
                }
            }

            remainingAmount = CalculateRemainingAmount(invoice.GrossAmount, settlements);
            List<Prepayment> updatedPrepayments = new List<Prepayment>();
            List<Settlement> newSetllements = new List<Settlement>();

            for (int k =0; k < counter; k++) 
            {
                var prepayment = _context.Prepayments.FirstOrDefault(p => p.PrepaymentId == selectedPrepayments[k]);
                decimal? settlementAmount = 0;

                prepayment.RemainingAmount -= remainingAmount;
                if (prepayment.RemainingAmount <= 0)
                {
                    settlementAmount = remainingAmount - prepayment.RemainingAmount;
                    prepayment.RemainingAmount = 0;
                    invoice.IsPaid = true;
                } else
                {
                    settlementAmount = remainingAmount;
                    prepayment.IsSettled = true;
                }

                var settlement = new Settlement
                {
                    InvoiceId = invoiceId,
                    SettlementDate = prepayment.PrepaymentDate,
                    SettlementAmount = settlementAmount,
                    PrepaymentId = prepayment.PrepaymentId
                };
                newSetllements.Add(settlement);
                updatedPrepayments.Add(prepayment);
            }

            _context.Settlements.AddRange(newSetllements);
            _context.Prepayments.UpdateRange(updatedPrepayments);
            _context.InvoicesLists.Update(invoice);
            _context.SaveChanges();


            // Przekieruj z powrotem do widoku Settlement
            return Redirect("/InvoicesList/Settlement/" + invoiceId);
        }

        [HttpGet]
        public IActionResult AddSettlement(int invoiceId)
        {
            var invoice = _context.InvoicesLists.FirstOrDefault(i => i.InvoiceId == invoiceId);
            decimal remainingAmount = (invoice.GrossAmount ?? 0) - _context.Settlements.Where(s => s.InvoiceId == invoiceId).Sum(s => s.SettlementAmount) ?? 0;

            var viewModel = new AddSettlementViewModel
            {
                InvoiceID = invoiceId,
                RemainingAmount = remainingAmount
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddSettlement(AddSettlementViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SettlementAmount <= model.RemainingAmount)
                {
                    var newSettlement = new Settlement
                    {
                        InvoiceId = model.InvoiceID,
                        SettlementDate = model.SettlementDate,
                        SettlementAmount = model.SettlementAmount,
                        PrepaymentId = model.PrepaymentID

                    };

                    _context.Settlements.Add(newSettlement);
                    _context.SaveChanges();

                    var invoice = _context.InvoicesLists.FirstOrDefault(i => i.InvoiceId == model.InvoiceID);
                    if (invoice != null)
                    {
                        var settlements = _context.Settlements.Where(s => s.InvoiceId == model.InvoiceID).ToList();
                        var remainingAmount = CalculateRemainingAmount(invoice.GrossAmount, settlements);

                        if(remainingAmount == 0)
                        {
                            invoice.IsPaid = true;
                            invoice.PaymentDate = _context.Settlements
                                .Where(s => s.InvoiceId == model.InvoiceID)
                                .OrderByDescending(s => s.SettlementDate)
                                .FirstOrDefault()?.SettlementDate;

                            _context.InvoicesLists.Update(invoice);
                            _context.SaveChanges();
                        }
                    } 

                        return Redirect("/InvoicesList/Settlement/" + model.InvoiceID);
                }
                else
                {
                    ModelState.AddModelError("", "Settlement amount cannot be greater than the remaining amount.");
                }
            }
            return View(model);
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
        public int ExtractFirstNumber(string registryNumber)
        {
            var parts = registryNumber.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[0], out int number))
            {
                return number;
            }
            return 0; // lub inna wartość domyślna w przypadku błędu parsowania
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

                if (invoicesList.InvoiceItems != null && invoicesList.InvoiceItems.Any())
                {
                    invoicesList.NetAmount = UpdateNetAmount(invoicesList.InvoiceItems);
                    invoicesList.GrossAmount = UpdateGrossAmount(invoicesList.InvoiceItems);
                }

                invoicesList.EntryDate = DateOnly.FromDateTime(DateTime.Now);

                //funkcja kolejny numer w bazi / datetime miesiac / rok
                int currentMonth = DateOnly.FromDateTime(DateTime.Now).Month;
                int currentYear = DateOnly.FromDateTime(DateTime.Now).Year;
                //var nextNumber = _context.InvoicesLists.Count(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == currentMonth && x.EntryDate.Value.Year == currentYear) + 1;

                var highestRegistryNumber = _context.InvoicesLists
                                                    .Where(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == currentMonth && x.EntryDate.Value.Year == currentYear)
                                                    .ToList()
                                                    .Select(x => ExtractFirstNumber(x.RegistryNumber))
                                                    .OrderByDescending(number => number)
                                                    .FirstOrDefault();



                var registryNumber = $@"{highestRegistryNumber+1}/{currentMonth}/{currentYear}";
                invoicesList.RegistryNumber = registryNumber;

                _context.Add(invoicesList);
                await _context.SaveChangesAsync();

                var tempDate = invoicesList.InvoiceDate.Value.AddDays(-1);
                var rateDate = _context.ToPlnRates
                                .Where(r => r.RateDate <= tempDate)
                                .OrderBy(r => r.RateDate)
                                .Last();

                invoicesList.RateDate = rateDate.RateDate;

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

        private decimal? UpdateNetAmount(List<InvoiceItem> invoiceItem)
        {
            decimal? netAmount = invoiceItem.Sum(x => x.NetAmount);
            return netAmount;
        }

        private decimal? UpdateGrossAmount(List<InvoiceItem> invoiceItem)
        {
            decimal? grossAmount = invoiceItem.Sum(x => x.GrossAmount);
            return grossAmount;
        }
    }
}
