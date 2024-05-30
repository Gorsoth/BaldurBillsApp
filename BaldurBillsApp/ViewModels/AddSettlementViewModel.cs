namespace BaldurBillsApp.ViewModels
{
    public class AddSettlementViewModel
    {
        public int InvoiceID { get; set; }
        public DateOnly SettlementDate { get; set; }
        public decimal SettlementAmount { get; set; }
        public int? PrepaymentID { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
