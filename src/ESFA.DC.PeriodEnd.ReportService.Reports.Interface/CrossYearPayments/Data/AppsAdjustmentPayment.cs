namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data
{
    public class AppsAdjustmentPayment
    {
        public string CollectionPeriodName { get; set; }

        public int AcademicYear { get; set; }

        public int CollectionPeriod { get; set; }

        public int PaymentType { get; set; }

        public decimal Amount { get; set; }
    }
}
