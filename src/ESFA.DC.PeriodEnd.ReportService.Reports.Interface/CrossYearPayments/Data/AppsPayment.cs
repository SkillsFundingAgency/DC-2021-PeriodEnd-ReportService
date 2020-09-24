namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.CrossYearPayments.Data
{
    public class AppsPayment
    {
        public int AcademicYear { get; set; }

        public int CollectionPeriod { get; set; }

        public string FundingLineType { get; set; }

        public int DeliveryPeriod { get; set; }

        public decimal Amount { get; set; }
    }
}
