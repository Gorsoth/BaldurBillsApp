using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using BaldurBillsApp.ViewModels;

namespace BaldurBillsApp.Controllers
{
    public class InvoicesListsController : Controller
    {
        private readonly BaldurBillsDbContext _context;

        public InvoicesListsController(BaldurBillsDbContext context)
        {
            _context = context;
        }

        // GET: InvoicesLists
        public async Task<IActionResult> Index()
        {
            var baldurBillsDbContext = _context.InvoicesLists.Include(i => i.Rate).Include(i => i.Vendor);
            return View(await baldurBillsDbContext.ToListAsync());
        }

        // GET: InvoicesLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var viewModel = new InvoicesListViewModel
            {
                InvoicesLists = await _context.InvoicesLists.ToListAsync()
            };
            return View(viewModel);
        }

        // GET: InvoicesLists/Create
        public IActionResult Create()
        {
            ViewData["RateId"] = new SelectList(_context.ToPlnRates, "RateId", "RateId");
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId");
            return View();
        }

        // POST: InvoicesLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceDate,VendorID,Title,NetAmount,GrossAmount,Currency,DueDate,IsPaid,PaymentDate,EntryDate,Comment,RateDate,RateID")] InvoicesList invoiceList)
        {
            if (ModelState.IsValid)
            {
                // Start a transaction
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Add the new invoice to the context without a RegistryNumber
                        _context.Add(invoiceList);
                        await _context.SaveChangesAsync();

                        // Retrieve the last RegistryNumber and increment it
                        var currentYear = DateTime.Now.Year;
                        var currentMonth = DateTime.Now.Month;

                        var lastInvoiceThisMonth = _context.InvoicesLists
                        .Where(inv => inv.InvoiceDate.HasValue &&
                        inv.InvoiceDate.Value.Year == currentYear &&
                        inv.InvoiceDate.Value.Month == currentMonth)
                        .OrderByDescending(inv => inv.RegistryNumber)
                        .FirstOrDefault();

                        int nextNumber = 1;
                        if (lastInvoiceThisMonth != null)
                        {
                            // Extract the number part of the RegistryNumber assuming it's the first part before '/'
                            int.TryParse(lastInvoiceThisMonth.RegistryNumber.Split('/')[0], out nextNumber);
                            nextNumber++; // Increment to get the next number
                        }

                        // Update the invoice with the new RegistryNumber
                        invoiceList.RegistryNumber = $"{nextNumber}/{currentMonth}/{currentYear}";
                        _context.Update(invoiceList);
                        await _context.SaveChangesAsync();

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        // Rollback the transaction if anything goes wrong
                        await transaction.RollbackAsync();
                        // Handle the error (log it, inform the user, etc.)
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(invoiceList);
        }

        // GET: InvoicesLists/Edit/5
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

        // POST: InvoicesLists/Edit/5
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

        // GET: InvoicesLists/Delete/5
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

        // POST: InvoicesLists/Delete/5
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
    }
}
