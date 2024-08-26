using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace BaldurBillsApp.Controllers
{
    [Authorize]
    public class ToPlnRatesController : Controller
    {
        private readonly BaldurBillsDbContext _context;

        public ToPlnRatesController(BaldurBillsDbContext context)
        {
            _context = context;
        }

        // GET: ToPlnRates/
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var lastDay = await _context.ToPlnRates.MaxAsync(r => (DateOnly?)r.RateDate);

            DateOnly selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            ViewBag.SelectedDate = selectedDate;

            if (date != null && date > lastDay)
            {
                date = lastDay;
            }

            if (date == null)
            {
                date = lastDay;
            }

            if (date == null)
            {
                return View(new List<ToPlnRate>());
            }



            var filteredRates = await _context.ToPlnRates
                        .Where(r => r.RateDate == date)
                        .ToListAsync();

            return View(filteredRates);
        }

        // GET: ToPlnRates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toPlnRate = await _context.ToPlnRates
                .FirstOrDefaultAsync(m => m.RateId == id);
            if (toPlnRate == null)
            {
                return NotFound();
            }

            return View(toPlnRate);
        }

        // GET: ToPlnRates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ToPlnRates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RateId,RateDate,RateCurrency,RateValue")] Models.ToPlnRate toPlnRate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(toPlnRate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toPlnRate);
        }

        // GET: ToPlnRates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toPlnRate = await _context.ToPlnRates.FindAsync(id);
            if (toPlnRate == null)
            {
                return NotFound();
            }
            return View(toPlnRate);
        }

        // POST: ToPlnRates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RateId,RateDate,RateCurrency,RateValue")] Models.ToPlnRate toPlnRate)
        {
            if (id != toPlnRate.RateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(toPlnRate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ToPlnRateExists(toPlnRate.RateId))
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
            return View(toPlnRate);
        }

        // GET: ToPlnRates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toPlnRate = await _context.ToPlnRates
                .FirstOrDefaultAsync(m => m.RateId == id);
            if (toPlnRate == null)
            {
                return NotFound();
            }

            return View(toPlnRate);
        }

        // POST: ToPlnRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toPlnRate = await _context.ToPlnRates.FindAsync(id);
            if (toPlnRate != null)
            {
                _context.ToPlnRates.Remove(toPlnRate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ToPlnRateExists(int id)
        {
            return _context.ToPlnRates.Any(e => e.RateId == id);
        }
    }
}
