namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model
{
    public class PaymentPeriodLines
    {
        public decimal Levy { get; set; }

        public decimal CoInvestment { get; set; }

        public decimal CoInvestmentDueFromEmployer { get; set; }

        public decimal EmployerAdditional { get; set; }

        public decimal ProviderAdditional { get; set; }

        public decimal ApprenticeAdditional { get; set; }

        public decimal EnglishAndMaths { get; set; }
        
        public decimal LearningSupportDisadvantageAndFrameworkUplifts { get; set; }

        public decimal Total { get; set; }
    }
}
