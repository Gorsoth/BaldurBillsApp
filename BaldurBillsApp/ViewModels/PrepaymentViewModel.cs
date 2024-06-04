namespace BaldurBillsApp.ViewModels
{
    public class PrepaymentViewModel
    {
        public int PrepaymentID { get; set; }

        public string? PrepaymentRegistryNumber { get; set; } = null!;
        public decimal? PrepaymentAmount { get; set; }

        public DateOnly? PrepaymentDate { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
    }
}
