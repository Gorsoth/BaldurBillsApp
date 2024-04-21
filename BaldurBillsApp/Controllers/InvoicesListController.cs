﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using BaldurBillsApp.Services;

namespace BaldurBillsApp.Controllers
{
    public class InvoicesListController : Controller
    {
        private readonly BaldurBillsDbContext _context;
        private readonly ISharedDataService _sharedDataService;

        public InvoicesListController(BaldurBillsDbContext context, ISharedDataService sharedDataService)
        {
            _context = context;
            _sharedDataService = sharedDataService;
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
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoicesList == null)
            {
                return NotFound();
            }

            return View(invoicesList);
        }

        // GET: InvoicesList/Create
        public IActionResult Create()
        {
            ViewBag.vendors = _sharedDataService.GetVendors();
            ViewBag.CurrencyList = _sharedDataService.GetCurrencies();
            return View();
        }

        // POST: InvoicesList/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,RegistryNumber,InvoiceDate,InvoiceNumber,VendorId,Title,NetAmount,GrossAmount,Currency,DueDate,IsPaid,PaymentDate,EntryDate,Comment,RateDate,RateId")] InvoicesList invoicesList)
        {
            if (ModelState.IsValid)
            {
                //funkcja kolejny numer w bazi / datetime miesiac / rok
                int currentMonth = DateOnly.FromDateTime(DateTime.Now).Month;
                int currentYear = DateOnly.FromDateTime(DateTime.Now).Year;

                var nextNumber = _context.InvoicesLists.Count(x => x.EntryDate.HasValue && x.EntryDate.Value.Month == currentMonth && x.EntryDate.Value.Year == currentYear) + 1;

                var registryNumber = $@"{nextNumber}/{currentMonth}/{currentYear}";
                invoicesList.RegistryNumber = registryNumber;

                _context.Add(invoicesList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
    }
}
