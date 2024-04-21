using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaldurBillsApp.Services
{
    public interface ISharedDataService
    {
    SelectList GetVendors();
    SelectList GetCurrencies();
 
    }
}
