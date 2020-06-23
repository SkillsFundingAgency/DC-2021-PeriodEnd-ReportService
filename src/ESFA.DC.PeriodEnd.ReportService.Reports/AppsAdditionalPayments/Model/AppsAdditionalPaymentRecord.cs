namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments.Model
{
    public class AppsAdditionalPaymentRecord
    {
        public RecordKey RecordKey { get; set; }

        public string FamilyName { get; set; }
        public string GivenNames { get; set; }
        public string ProviderSpecifiedLearnerMonitoringA { get; set; }
        public string ProviderSpecifiedLearnerMonitoringB { get; set; }

        public EarningsAndPayments EarningsAndPayments { get; set; }
    }
}