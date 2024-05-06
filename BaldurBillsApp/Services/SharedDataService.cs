using BaldurBillsApp.Models;
using BaldurBillsApp.SelectModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BaldurBillsApp.Services
{
    public class SharedDataService : ISharedDataService
    {
        private readonly BaldurBillsDbContext _context;

        public SharedDataService(BaldurBillsDbContext context)
        {
            _context = context;
        }

        public SelectList GetVendors()
        {
            var vendors = _context.Vendors
                           .Select(v => new { Text = v.VendorName + " - " + v.VatId, Value = v.VendorId })
                           .ToList();
            return new SelectList(vendors, "Value", "Text");
        }

        public SelectList GetCurrencies()
        {
            var currencies = _context.ToPlnRates
                            .Select(r => r.RateCurrency)
                            .Distinct()
                            .Select(r => new CurrencySelectModel
                            {
                                CurrencyName = r
                            })
                            .ToList();
            return new SelectList(currencies, "CurrencyName", "CurrencyName");
        }

        public SelectList GetCostTypes()
        {
            var costTypes = _context.CostTypes
                            .Select(c => new { Text = c.CostName, Value = c.CostId })
                           .ToList();
            return new SelectList(costTypes, "Value", "Text");
        }
    }
}
