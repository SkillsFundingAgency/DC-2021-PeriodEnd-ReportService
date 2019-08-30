namespace ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.PeriodEndMetrics
{
    public class IlrMetrics
    {
        public string TransactionType { get; set; }

        public decimal EarningsYTD { get; set; }

        public decimal EarningsACT1 { get; set; }

        public decimal EarningsACT2 { get; set; }
    }
}