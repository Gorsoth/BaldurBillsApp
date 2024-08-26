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
    public class CostTypesController : Controller
    {
        private readonly BaldurBillsDbContext _context;

        public CostTypesController(BaldurBillsDbContext context)
        {
            _context = context;
        }

        // GET: CostTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.CostTypes.ToListAsync());
        }

        // GET: CostTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var costType = await _context.CostTypes
                .FirstOrDefaultAsync(m => m.CostId == id);
            if (costType == null)
            {
                return NotFound();
            }

            return View(costType);
        }

        // GET: CostTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CostTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CostId,CostName")] CostType costType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(costType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(costType);
        }

        // GET: CostTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var costType = await _context.CostTypes.FindAsync(id);
            if (costType == null)
            {
                return NotFound();
            }
            return View(costType);
        }

        // POST: CostTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CostId,CostName")] CostType costType)
        {
            if (id != costType.CostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(costType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CostTypeExists(costType.CostId))
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
            return View(costType);
        }

        // GET: CostTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var costType = await _context.CostTypes
                .FirstOrDefaultAsync(m => m.CostId == id);
            if (costType == null)
            {
                return NotFound();
            }

            return View(costType);
        }

        // POST: CostTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var costType = await _context.CostTypes.FindAsync(id);
            if (costType != null)
            {
                _context.CostTypes.Remove(costType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CostTypeExists(int id)
        {
            return _context.CostTypes.Any(e => e.CostId == id);
        }
    }
}
