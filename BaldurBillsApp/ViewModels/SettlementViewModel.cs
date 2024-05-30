using BaldurBillsApp.Models;
namespace BaldurBillsApp.ViewModels

{
    public class SettlementViewModel
    {
        public InvoicesList InvoiceList { get; set; }
        public List<Settlement> Settlements { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
