using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using BaldurBillsApp.SelectModels;
using BaldurBillsApp.Services;

namespace BaldurBillsApp.Controllers
{
    public class PrepaymentsController : Controller
    {
        private readonly BaldurBillsDbContext _context;
        private readonly ISharedDataService _sharedDataService;

        public PrepaymentsController(BaldurBillsDbContext context, ISharedDataService sharedDataService)
        {
            _context = context;
            _sharedDataService = sharedDataService;
        }

        // GET: Prepayments
        public async Task<IActionResult> Index()
        {
            var baldurBillsDbContext = _context.Prepayments.Include(p => p.Vendor);
            return View(await baldurBillsDbContext.ToListAsync());
        }

        // GET: Prepayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prepayment = await _context.Prepayments
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PrepaymentId == id);
            if (prepayment == null)
            {
                return NotFound();
            }

            return View(prepayment);
        }

        // GET: Prepayments/Create
        public IActionResult Create()
        {
            ViewBag.vendors = _sharedDataService.GetVendors();
            ViewBag.CurrencyList = _sharedDataService.GetCurrencies();
            return View();
        }

        // POST: Prepayments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrepaymentId,PrepaymentRegistryNumber,VendorId,PrepaymentAmount,PrepaymentCurrency,PrepaymentDate,RemainingAmount,IsSettled,PrepaymentEntryDate")] Prepayment prepayment)
        {
            


            if (ModelState.IsValid)
            {
                prepayment.PrepaymentEntryDate = DateOnly.FromDateTime(DateTime.Now);

                //funkcja kolejny numer w bazi / datetime miesiac / rok
                int currentMonth = DateOnly.FromDateTime(DateTime.Now).Month;
                int currentYear = DateOnly.FromDateTime(DateTime.Now).Year;

                var nextNumber = _context.Prepayments.Count(x => x.PrepaymentEntryDate.HasValue && x.PrepaymentEntryDate.Value.Month == currentMonth && x.PrepaymentEntryDate.Value.Year == currentYear) + 1;

                var registryNumber = $@"{nextNumber}/{currentMonth}/{currentYear}";
                prepayment.PrepaymentRegistryNumber = registryNumber;

                _context.Add(prepayment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", prepayment.VendorId);
            ViewBag.Vendors = _context.Vendors.ToList();
            return View(prepayment);
        }

        // GET: Prepayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prepayment = await _context.Prepayments.FindAsync(id);
            if (prepayment == null)
            {
                return NotFound();
            }
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", prepayment.VendorId);
            return View(prepayment);
        }

        // POST: Prepayments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PrepaymentId,PrepaymentRegistryNumber,VendorId,PrepaymentAmount,PrepaymentCurrency,PrepaymentDate,RemainingAmount,IsSettled,PrepaymentEntryDate")] Prepayment prepayment)
        {
            if (id != prepayment.PrepaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prepayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrepaymentExists(prepayment.PrepaymentId))
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
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "VendorId", prepayment.VendorId);
            return View(prepayment);
        }

        // Settlement subtraction
        //public async Task<IActionResult> SubtractSettlement(int? id)
        //{

        //}

        // GET: Prepayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prepayment = await _context.Prepayments
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PrepaymentId == id);
            if (prepayment == null)
            {
                return NotFound();
            }

            return View(prepayment);
        }

        // POST: Prepayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prepayment = await _context.Prepayments.FindAsync(id);
            if (prepayment != null)
            {
                _context.Prepayments.Remove(prepayment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrepaymentExists(int id)
        {
            return _context.Prepayments.Any(e => e.PrepaymentId == id);
        }
    }
}
